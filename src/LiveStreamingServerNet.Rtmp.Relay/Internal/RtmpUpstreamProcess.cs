using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal
{
    internal class RtmpUpstreamProcess : IRtmpUpstreamProcess
    {
        private readonly string _streamPath;
        private readonly IReadOnlyDictionary<string, string> _streamArguments;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly RtmpUpstreamConfiguration _config;
        private readonly ILogger _logger;

        private readonly TaskCompletionSource _initializationComplete;
        private readonly IPacketDiscarder _packetDiscarder;
        private readonly Channel<MediaData> _mediaDataChannel;

        private uint _audioTimestamp;
        private uint _videoTimestamp;
        private long _outstandingPacketsSize;
        private long _outstandingPacketCount;

        public string StreamPath => _streamPath;
        public IReadOnlyDictionary<string, string> StreamArguments => _streamArguments;

        public RtmpUpstreamProcess(
            IRtmpPublishStreamContext publishStreamContext,
            IRtmpOriginResolver originResolver,
            IBufferPool bufferPool,
            IDataBufferPool dataBufferPool,
            IUpstreamMediaPacketDiscarderFactory packetDiscarderFactory,
            IOptions<RtmpUpstreamConfiguration> config,
            ILogger<RtmpUpstreamProcess> logger)
        {
            _streamPath = publishStreamContext.StreamPath;
            _streamArguments = publishStreamContext.StreamArguments;
            _publishStreamContext = publishStreamContext;
            _originResolver = originResolver;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config.Value;
            _logger = logger;

            _initializationComplete = new TaskCompletionSource();
            _packetDiscarder = packetDiscarderFactory.Create(_streamPath);
            _mediaDataChannel = CreateMediaDataChannel();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await RunUpstreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamError(_streamPath, ex);
            }
        }

        private async Task RunUpstreamAsync(CancellationToken stoppingToken)
        {
            using var abortCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            var origin = await _originResolver.ResolveUpstreamOriginAsync(_streamPath, _streamArguments, abortCts.Token);

            if (origin == null)
            {
                return;
            }

            _logger.RtmpUpstreamOriginResolved(_streamPath, origin);

            await RunUpstreamClientAsync(origin, abortCts);
        }

        private async Task RunUpstreamClientAsync(RtmpOrigin origin, CancellationTokenSource abortCts)
        {
            try
            {
                await using var rtmpClient = CreateUpstreamClient();
                using var _ = abortCts.Token.Register(rtmpClient.Stop);

                _logger.RtmpUpstreamConnecting(_streamPath, origin.EndPoint);
                await rtmpClient.ConnectAsync(origin.EndPoint, origin.AppName);

                _logger.RtmpUpstreamCreating(_streamPath);
                var rtmpStream = await rtmpClient.CreateStreamAsync();

                using var idleChecker = CreateIdleCheck(abortCts);

                SubscribeToStreamEvents(rtmpStream, idleChecker, abortCts);

                var mediaDataSendingTask = MediaDataSendingTask(rtmpStream, idleChecker, abortCts);
                rtmpStream.Publish.Publish(origin.StreamName);

                _logger.RtmpUpstreamCreated(_streamPath);
                var completedTask = await Task.WhenAny(rtmpClient.UntilStoppedAsync(), mediaDataSendingTask);
                await completedTask;
            }
            catch (OperationCanceledException) when (abortCts.IsCancellationRequested) { }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _logger.RtmpUpstreamStopped(_streamPath);
                abortCts.Cancel();
            }
        }

        private IRtmpClient CreateUpstreamClient()
        {
            IRtmpClientBuilder builder = RtmpClientBuilder.Create();

            if (_config.ConfigureRtmpUpstreamClient != null)
            {
                builder = builder.ConfigureRtmpClient(_config.ConfigureRtmpUpstreamClient);
            }

            builder = builder.ConfigureClient((configurator) =>
            {
                if (_config.ConfigureUpstreamClient != null)
                    _config.ConfigureUpstreamClient(configurator);

                configurator.Services.TryAddSingleton(_dataBufferPool);
                configurator.Services.TryAddSingleton(_bufferPool);
            });

            return builder.Build();
        }

        private void SubscribeToStreamEvents(IRtmpStream rtmpStream, IIdleChecker idleChecker, CancellationTokenSource abortCts)
        {
            rtmpStream.OnStatusReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Level == RtmpStatusLevels.Error)
                {
                    abortCts.Cancel();
                }
                else if (eventArgs.Code == RtmpStreamStatusCodes.PublishStart)
                {
                    idleChecker.Refresh();
                    _ = InitializeUpstreamAsync(rtmpStream, abortCts);
                }
            };
        }

        private async ValueTask InitializeUpstreamAsync(IRtmpStream rtmpStream, CancellationTokenSource abortCts)
        {
            try
            {
                if (_publishStreamContext.AudioSequenceHeader != null)
                {
                    await SendMediaSequenceHeaderAsync(rtmpStream, MediaType.Audio, _publishStreamContext.AudioSequenceHeader);
                }

                if (_publishStreamContext.VideoSequenceHeader != null)
                {
                    await SendMediaSequenceHeaderAsync(rtmpStream, MediaType.Video, _publishStreamContext.VideoSequenceHeader);
                }

                var pictures = _publishStreamContext.GroupOfPicturesCache.Get();
                try
                {
                    foreach (var picture in pictures)
                    {
                        if (UpdateTimestamp(picture.Type, picture.Timestamp))
                        {
                            await SendMediaDataAsync(rtmpStream, new MediaData(picture.Type, picture.Timestamp, true, picture.Payload));
                        }
                    }
                }
                finally
                {
                    foreach (var picture in pictures)
                        picture.Payload.Unclaim();
                }

                _initializationComplete.SetResult();
            }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamInitializationError(_streamPath, ex);
                abortCts.Cancel();
            }

            async ValueTask SendMediaSequenceHeaderAsync(IRtmpStream rtmpStream, MediaType mediaType, byte[] sequenceHeader)
            {
                var buffer = new RentedBuffer(sequenceHeader.Length);

                try
                {
                    sequenceHeader.AsSpan().CopyTo(buffer.Buffer);
                    await SendMediaDataAsync(rtmpStream, new MediaData(mediaType, 0, false, buffer));
                }
                finally
                {
                    buffer.Unclaim();
                }
            }
        }

        private async Task MediaDataSendingTask(IRtmpStream rtmpStream, IIdleChecker idleChecker, CancellationTokenSource abortCts)
        {
            try
            {
                var isInitializationComplete = false;
                var cancellationToken = abortCts.Token;

                while (!abortCts.IsCancellationRequested)
                {
                    isInitializationComplete = await UntilInitializationCompleteAsync(isInitializationComplete, cancellationToken);

                    var mediaData = await DequeueMediaData(cancellationToken);
                    idleChecker.Refresh();

                    try
                    {
                        if (!UpdateTimestamp(mediaData.Type, mediaData.Timestamp) && mediaData.IsSkippable)
                        {
                            continue;
                        }

                        await SendMediaDataAsync(rtmpStream, mediaData);
                    }
                    catch (Exception ex)
                    {
                        _logger.RtmpUpstreamMediaDataSendingError(_streamPath, mediaData.Type, ex);
                        throw;
                    }
                    finally
                    {
                        mediaData.Payload.Unclaim();
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
                _mediaDataChannel.Writer.Complete();

                while (_mediaDataChannel.Reader.TryRead(out var mediaData))
                {
                    mediaData.Payload.Unclaim();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            async ValueTask<bool> UntilInitializationCompleteAsync(
                bool isInitializationComplete, CancellationToken cancellationToken)
            {
                if (!isInitializationComplete)
                {
                    await _initializationComplete.Task.WithCancellation(cancellationToken);
                    isInitializationComplete = true;
                }

                return isInitializationComplete;
            }
        }

        private static async Task SendMediaDataAsync(IRtmpStream rtmpStream, MediaData mediaData)
        {
            switch (mediaData.Type)
            {
                case MediaType.Audio:
                    await rtmpStream.Publish.SendAudioDataAsync(mediaData.Payload, mediaData.Timestamp);
                    break;

                case MediaType.Video:
                    await rtmpStream.Publish.SendVideoDataAsync(mediaData.Payload, mediaData.Timestamp);
                    break;

                default:
                    break;
            }
        }

        private bool UpdateTimestamp(MediaType mediaType, uint timestamp)
        {
            return mediaType switch
            {
                MediaType.Audio => DoUpdateTimestamp(ref _audioTimestamp, timestamp),
                MediaType.Video => DoUpdateTimestamp(ref _videoTimestamp, timestamp),
                _ => false,
            };

            bool DoUpdateTimestamp(ref uint currentTimestamp, uint newTimestamp)
            {
                while (true)
                {
                    var original = currentTimestamp;

                    if (original > 0 && newTimestamp <= original)
                    {
                        return false;
                    }

                    if (Interlocked.CompareExchange(ref currentTimestamp, newTimestamp, original) == original)
                    {
                        return true;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnqueueMediaData(MediaData mediaData)
        {
            if (_packetDiscarder.ShouldDiscardPacket(
                mediaData.IsSkippable, _outstandingPacketsSize, _outstandingPacketCount))
            {
                return;
            }

            try
            {
                mediaData.Payload.Claim();

                if (!_mediaDataChannel.Writer.TryWrite(mediaData))
                {
                    throw new ChannelClosedException();
                }

                Interlocked.Add(ref _outstandingPacketsSize, mediaData.Payload.Size);
                Interlocked.Increment(ref _outstandingPacketCount);
            }
            catch
            {
                mediaData.Payload.Unclaim();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<MediaData> DequeueMediaData(CancellationToken cancellationToken)
        {
            var mediaData = await _mediaDataChannel.Reader.ReadAsync(cancellationToken);
            Interlocked.Add(ref _outstandingPacketsSize, -mediaData.Payload.Size);
            Interlocked.Decrement(ref _outstandingPacketCount);
            return mediaData;
        }

        public void OnReceiveMediaData(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            EnqueueMediaData(new MediaData(mediaType, timestamp, isSkippable, rentedBuffer));
        }

        private IIdleChecker CreateIdleCheck(CancellationTokenSource abortCts)
        {
            void OnIdleTimeout()
            {
                abortCts.Cancel();
                _logger.RtmpUpstreamIdleTimeout(_streamPath);
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

        private record struct MediaData(MediaType Type, uint Timestamp, bool IsSkippable, IRentedBuffer Payload);
    }
}
