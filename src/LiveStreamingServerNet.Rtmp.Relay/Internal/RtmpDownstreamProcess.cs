using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal
{
    internal class RtmpDownstreamProcess : IRtmpDownstreamProcess
    {
        private readonly string _streamPath;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpStreamDeletionService _streamDeletion;
        private readonly IRtmpVideoDataProcessorService _videoDataProcessor;
        private readonly IRtmpAudioDataProcessorService _audioDataProcessor;
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly RtmpDownstreamConfiguration _config;
        private readonly ILogger _logger;

        public RtmpDownstreamProcess(
            string streamPath,
            IRtmpStreamManagerService streamManager,
            IRtmpStreamDeletionService streamDeletion,
            IRtmpVideoDataProcessorService videoDataProcessor,
            IRtmpAudioDataProcessorService audioDataProcessor,
            IRtmpOriginResolver originResolver,
            IBufferPool bufferPool,
            IDataBufferPool dataBufferPool,
            IOptions<RtmpDownstreamConfiguration> config,
            ILogger<RtmpDownstreamProcess> logger)
        {
            _streamPath = streamPath;
            _streamManager = streamManager;
            _streamDeletion = streamDeletion;
            _videoDataProcessor = videoDataProcessor;
            _audioDataProcessor = audioDataProcessor;
            _originResolver = originResolver;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config.Value;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var publishStreamContext = CreatePublishStreamContext();
            var publishingResult = _streamManager.StartDirectPublishing(publishStreamContext, out _);

            if (publishingResult != PublishingStreamResult.Succeeded)
                return;

            try
            {
                await RunDownstreamAsync(publishStreamContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.RtmpDownstreamError(publishStreamContext.StreamPath, ex);
            }
            finally
            {
                _streamManager.StopPublishing(publishStreamContext, out var subscribers);

                await Task.WhenAll(subscribers
                    .Select(subscriber => subscriber.StreamContext)
                    .Select(streamContext => _streamDeletion.DeleteStreamAsync(streamContext).AsTask())
                );
            }
        }

        private async Task RunDownstreamAsync(IRtmpPublishStreamContext publishStreamContext, CancellationToken stoppingToken)
        {
            using var abortCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            var origin = await _originResolver.ResolveDownstreamOriginAsync(publishStreamContext.StreamPath, abortCts.Token);

            if (origin == null)
            {
                return;
            }

            _logger.RtmpDownstreamOriginResolved(publishStreamContext.StreamPath, origin);

            var mediaDataChannel = CreateMediaDataChannel();
            var mediaDataProcessorTask = MediaDataProcessorTask(mediaDataChannel, publishStreamContext, abortCts);
            var downstreamClientTask = RunDownstreamClientAsync(origin, mediaDataChannel, abortCts);

            await Task.WhenAll(downstreamClientTask, mediaDataProcessorTask);
        }

        private async Task RunDownstreamClientAsync(RtmpOrigin origin, Channel<MediaData> mediaDataChannel, CancellationTokenSource abortCts)
        {
            try
            {
                await using var rtmpClient = CreateDownstreamClient();
                using var _ = abortCts.Token.Register(rtmpClient.Stop);

                _logger.RtmpDownstreamConnecting(_streamPath, origin.EndPoint);
                await rtmpClient.ConnectAsync(origin.EndPoint, origin.AppName);

                _logger.RtmpDownstreamCreating(_streamPath);
                var rtmpStream = await rtmpClient.CreateStreamAsync();

                using var idleChecker = CreateIdleCheck(abortCts);

                SubscribeToStreamEvents(rtmpStream, idleChecker, mediaDataChannel, abortCts);
                rtmpStream.Subscribe.Play(origin.StreamName);

                _logger.RtmpDownstreamCreated(_streamPath);
                await rtmpClient.UntilStoppedAsync();
            }
            catch (OperationCanceledException) when (abortCts.IsCancellationRequested) { }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _logger.RtmpDownstreamStopped(_streamPath);
                abortCts.Cancel();
            }
        }

        private IRtmpClient CreateDownstreamClient()
        {
            IRtmpClientBuilder builder = RtmpClientBuilder.Create();

            if (_config.ConfigureRtmpDownstreamClient != null)
            {
                builder = builder.ConfigureRtmpClient(_config.ConfigureRtmpDownstreamClient);
            }

            builder = builder.ConfigureClient((configurator) =>
            {
                configurator.Services.AddSingleton(_dataBufferPool);
                configurator.Services.AddSingleton(_bufferPool);

                if (_config.ConfigureDownstreamClient != null)
                    _config.ConfigureDownstreamClient(configurator);
            });

            return builder.Build();
        }

        private void SubscribeToStreamEvents(
            IRtmpStream rtmpStream, IIdleChecker idleChecker, Channel<MediaData> mediaDataChannel, CancellationTokenSource abortCts)
        {
            rtmpStream.Subscribe.OnVideoDataReceived += (sender, eventArgs) =>
            {
                idleChecker.Refresh();
                EnqueueMediaData(MediaType.Video, mediaDataChannel.Writer, eventArgs);
            };

            rtmpStream.Subscribe.OnAudioDataReceived += (sender, eventArgs) =>
            {
                idleChecker.Refresh();
                EnqueueMediaData(MediaType.Audio, mediaDataChannel.Writer, eventArgs);
            };

            rtmpStream.OnUserControlEventReceived += (sender, eventArgs) =>
            {
                switch (eventArgs.EventType)
                {
                    case UserControlEventType.StreamBegin:
                        idleChecker.Refresh();
                        break;

                    case UserControlEventType.StreamEOF:
                        abortCts.Cancel();
                        break;

                    default:
                        break;
                }
            };

            rtmpStream.OnStatusReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Level == RtmpStatusLevels.Error)
                {
                    abortCts.Cancel();
                    return;
                }

                switch (eventArgs.Code)
                {
                    case RtmpStreamStatusCodes.PlayStart:
                        idleChecker.Refresh();
                        break;

                    case RtmpStreamStatusCodes.PlayUnpublishNotify:
                        abortCts.Cancel();
                        break;

                    default:
                        break;
                }
            };
        }

        private async Task MediaDataProcessorTask(
            Channel<MediaData> channel, IRtmpPublishStreamContext publishStreamContext, CancellationTokenSource abortCts)
        {
            try
            {
                var cancellationToken = abortCts.Token;

                while (!abortCts.IsCancellationRequested)
                {
                    var mediaData = await DequeueMediaData(channel.Reader, cancellationToken);

                    try
                    {
                        var success = mediaData.Type switch
                        {
                            MediaType.Audio => await _audioDataProcessor.ProcessAudioDataAsync(
                                publishStreamContext, mediaData.Timestamp, mediaData.Payload),

                            MediaType.Video => await _videoDataProcessor.ProcessVideoDataAsync(
                                publishStreamContext, mediaData.Timestamp, mediaData.Payload),

                            _ => false
                        };

                        if (!success)
                        {
                            _logger.RtmpDownstreamMediaDataProcessingError(publishStreamContext.StreamPath, mediaData.Type);
                            return;
                        }
                    }
                    finally
                    {
                        _dataBufferPool.Recycle(mediaData.Payload);
                    }
                }
            }
            catch (OperationCanceledException) when (abortCts.IsCancellationRequested) { }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                abortCts.Cancel();
                channel.Writer.Complete();

                while (channel.Reader.TryRead(out var mediaData))
                {
                    _dataBufferPool.Recycle(mediaData.Payload);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueMediaData(MediaType mediaType, ChannelWriter<MediaData> channel, MediaDataEventArgs eventArgs)
        {
            var payloadBuffer = _dataBufferPool.Obtain();

            try
            {
                payloadBuffer.Write(eventArgs.RentedBuffer.AsSpan());
                payloadBuffer.MoveTo(0);

                if (!channel.TryWrite(new MediaData(mediaType, eventArgs.Timestamp, payloadBuffer)))
                {
                    throw new ChannelClosedException();
                }
            }
            catch
            {
                _dataBufferPool.Recycle(payloadBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<MediaData> DequeueMediaData(ChannelReader<MediaData> channel, CancellationToken cancellationToken)
        {
            return channel.ReadAsync(cancellationToken);
        }

        private IIdleChecker CreateIdleCheck(CancellationTokenSource abortCts)
        {
            void OnIdleTimeout()
            {
                abortCts.Cancel();
                _logger.RtmpDownstreamIdleTimeout(_streamPath);
            }

            return new IdleChecker(_config.IdleCheckingInterval, _config.MaximumIdleTime, OnIdleTimeout);
        }

        private RtmpPublishStreamContext CreatePublishStreamContext()
        {
            return new RtmpPublishStreamContext(null, _streamPath, new Dictionary<string, string>(), _bufferPool);
        }

        private static Channel<MediaData> CreateMediaDataChannel()
        {
            return Channel.CreateUnbounded<MediaData>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });
        }

        private record struct MediaData(MediaType Type, uint Timestamp, IDataBuffer Payload);
    }
}
