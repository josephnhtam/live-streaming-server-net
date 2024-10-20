using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Streams
{
    internal class RtmpDownstreamProcess : IRtmpDownstreamProcess
    {
        private readonly string _streamPath;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpStreamDeletionService _streamDeletion;
        private readonly IRtmpVideoDataProcessorService _videoDataProcessor;
        private readonly IRtmpAudioDataProcessorService _audioDataProcessor;
        private readonly IRtmpMetaDataProcessorService _metaDataProcessor;
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly RtmpDownstreamConfiguration _config;
        private readonly ILogger _logger;

        private RtmpPublishStreamContext? _publishStreamContext;
        private bool initialized;

        public string StreamPath => _streamPath;

        public RtmpDownstreamProcess(
            string streamPath,
            IRtmpStreamManagerService streamManager,
            IRtmpStreamDeletionService streamDeletion,
            IRtmpVideoDataProcessorService videoDataProcessor,
            IRtmpAudioDataProcessorService audioDataProcessor,
            IRtmpMetaDataProcessorService metaDataProcessor,
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
            _metaDataProcessor = metaDataProcessor;
            _originResolver = originResolver;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config.Value;
            _logger = logger;
        }

        public async ValueTask<PublishingStreamResult> InitializeAsync(CancellationToken cancellationToken)
        {
            if (initialized)
                throw new InvalidOperationException("The process has already been initialized.");


            try
            {
                var publishStreamContext = CreatePublishStreamContext();
                var publishingResult = await _streamManager.StartDirectPublishingAsync(publishStreamContext);

                if (publishingResult.Result == PublishingStreamResult.Succeeded)
                {
                    _publishStreamContext = publishStreamContext;
                    initialized = true;
                }

                return publishingResult.Result;
            }
            catch (Exception ex)
            {
                _logger.RtmpDownstreamInitializationError(_streamPath, ex);
                throw;
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (!initialized)
                throw new InvalidOperationException("The process has not been initialized.");

            Debug.Assert(_publishStreamContext != null);

            try
            {
                await RunDownstreamAsync(_publishStreamContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.RtmpDownstreamError(_publishStreamContext.StreamPath, ex);
            }
        }

        private async ValueTask<RtmpOrigin?> ResolveOriginAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _originResolver.ResolveDownstreamOriginAsync(_streamPath, cancellationToken);

                if (result == null)
                {
                    _logger.RtmpDownstreamOriginNotResolved(_streamPath);
                }
                else
                {
                    _logger.RtmpDownstreamOriginResolved(_streamPath, result);
                }

                return result;
            }
            catch
            {
                _logger.RtmpDownstreamOriginNotResolved(_streamPath);
                return null;
            }
        }

        private async Task RunDownstreamAsync(IRtmpPublishStreamContext publishStreamContext, CancellationToken stoppingToken)
        {
            var origin = await ResolveOriginAsync(stoppingToken);
            if (origin == null) return;

            using var abortCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            var streamDataChannel = CreateStreamDataChannel();
            var mediaDataProcessorTask = StreamDataProcessorTask(streamDataChannel, publishStreamContext, abortCts);
            var downstreamClientTask = RunDownstreamClientAsync(origin, streamDataChannel, abortCts);

            await Task.WhenAll(downstreamClientTask, mediaDataProcessorTask);
        }

        private async Task RunDownstreamClientAsync(RtmpOrigin origin, Channel<StreamData> streamDataChannel, CancellationTokenSource abortCts)
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

                SubscribeToStreamEvents(rtmpStream, idleChecker, streamDataChannel, abortCts);
                rtmpStream.Subscribe.Play(origin.StreamName);

                _logger.RtmpDownstreamCreated(_streamPath);
                await rtmpClient.UntilStoppedAsync(abortCts.Token);
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
                if (_config.ConfigureDownstreamClient != null)
                    _config.ConfigureDownstreamClient(configurator);

                configurator.Services.TryAddSingleton(_dataBufferPool);
                configurator.Services.TryAddSingleton(_bufferPool);
            });

            return builder.Build();
        }

        private void SubscribeToStreamEvents(
            IRtmpStream rtmpStream, IIdleChecker idleChecker, Channel<StreamData> streamDataChannel, CancellationTokenSource abortCts)
        {
            rtmpStream.Subscribe.OnStreamMetaDataReceived += (sender, eventArgs) =>
            {
                idleChecker.Refresh();
                EnqueueMetaData(streamDataChannel.Writer, eventArgs);
            };

            rtmpStream.Subscribe.OnVideoDataReceived += (sender, eventArgs) =>
            {
                idleChecker.Refresh();
                EnqueueMediaData(MediaType.Video, streamDataChannel.Writer, eventArgs);
            };

            rtmpStream.Subscribe.OnAudioDataReceived += (sender, eventArgs) =>
            {
                idleChecker.Refresh();
                EnqueueMediaData(MediaType.Audio, streamDataChannel.Writer, eventArgs);
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

        private async Task StreamDataProcessorTask(
            Channel<StreamData> channel, IRtmpPublishStreamContext publishStreamContext, CancellationTokenSource abortCts)
        {
            try
            {
                var cancellationToken = abortCts.Token;

                while (!abortCts.IsCancellationRequested)
                {
                    var streamData = await DequeueStreamData(channel.Reader, cancellationToken);

                    if (streamData.MediaData.HasValue)
                    {
                        await ProcessMediaDataAsync(publishStreamContext, streamData.MediaData.Value, abortCts);
                    }
                    else if (streamData.MetaData.HasValue)
                    {
                        await _metaDataProcessor.ProcessMetaDataAsync(publishStreamContext, 0u, streamData.MetaData.Value.StreamMetaData);
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

                while (channel.Reader.TryRead(out var streamData))
                {
                    if (streamData.MediaData.HasValue)
                        _dataBufferPool.Recycle(streamData.MediaData.Value.Payload);
                }
            }
        }

        private async Task ProcessMediaDataAsync(IRtmpPublishStreamContext publishStreamContext, MediaData mediaData, CancellationTokenSource abortCts)
        {
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
                    abortCts.Cancel();
                    return;
                }
            }
            finally
            {
                _dataBufferPool.Recycle(mediaData.Payload);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (_publishStreamContext == null)
                return;

            _ = _metaDataProcessor.ProcessMetaDataAsync(_publishStreamContext, 0u, metaData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueMediaData(MediaType mediaType, ChannelWriter<StreamData> channel, MediaDataEventArgs eventArgs)
        {
            var payloadBuffer = _dataBufferPool.Obtain();

            try
            {
                payloadBuffer.Write(eventArgs.RentedBuffer.AsSpan());
                payloadBuffer.MoveTo(0);

                if (!channel.TryWrite(new StreamData(new MediaData(mediaType, eventArgs.Timestamp, payloadBuffer))))
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
        private void EnqueueMetaData(ChannelWriter<StreamData> channel, StreamMetaDataEventArgs eventArgs)
        {
            channel.TryWrite(new StreamData(new MetaData(eventArgs.StreamMetaData)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<StreamData> DequeueStreamData(ChannelReader<StreamData> channel, CancellationToken cancellationToken)
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

        private static Channel<StreamData> CreateStreamDataChannel()
        {
            return Channel.CreateUnbounded<StreamData>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });
        }

        public async ValueTask DisposeAsync()
        {
            if (_publishStreamContext == null)
                return;

            var stopPublishingResult = await _streamManager.StopPublishingAsync(_publishStreamContext, false);

            await Task.WhenAll(stopPublishingResult.SubscribeStreamContexts
                .Select(subscriber => subscriber.StreamContext)
                .Select(streamContext => _streamDeletion.DeleteStreamAsync(streamContext).AsTask())
            );

            _publishStreamContext.Dispose();
        }

        private record struct StreamData(MediaData? MediaData, MetaData? MetaData)
        {
            public StreamData(MediaData mediaData) : this(mediaData, null) { }
            public StreamData(MetaData metaData) : this(null, metaData) { }
        }

        private record struct MediaData(MediaType Type, uint Timestamp, IDataBuffer Payload);
        private record struct MetaData(IReadOnlyDictionary<string, object> StreamMetaData);
    }
}
