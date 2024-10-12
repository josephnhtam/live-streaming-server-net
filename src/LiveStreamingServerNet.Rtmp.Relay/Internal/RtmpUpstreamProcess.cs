using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly RtmpUpstreamConfiguration _config;
        private readonly ILogger _logger;

        private readonly IPacketDiscarder _packetDiscarder;
        private readonly Channel<MediaData> _mediaDataChannel;

        private bool _publishStarted;
        private long _outstandingPacketsSize;
        private long _outstandingPacketCount;

        public string StreamPath => _streamPath;
        public IReadOnlyDictionary<string, string> StreamArguments => _streamArguments;

        public RtmpUpstreamProcess(
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            IRtmpOriginResolver originResolver,
            IBufferPool bufferPool,
            IDataBufferPool dataBufferPool,
            IUpstreamMediaPacketDiscarderFactory packetDiscarderFactory,
            IOptions<RtmpUpstreamConfiguration> config,
            ILogger<RtmpUpstreamProcess> logger)
        {
            _streamPath = streamPath;
            _streamArguments = new Dictionary<string, string>(streamArguments);
            _originResolver = originResolver;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config.Value;
            _logger = logger;

            _packetDiscarder = packetDiscarderFactory.Create(streamPath);
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
                    return;
                }

                if (eventArgs.Code == RtmpStreamStatusCodes.PublishStart)
                {
                    idleChecker.Refresh();
                    _publishStarted = true;
                    return;
                }
            };
        }

        private async Task MediaDataSendingTask(IRtmpStream rtmpStream, IIdleChecker idleChecker, CancellationTokenSource abortCts)
        {
            try
            {
                var cancellationToken = abortCts.Token;

                while (!abortCts.IsCancellationRequested)
                {
                    var mediaData = await DequeueMediaData(cancellationToken);
                    idleChecker.Refresh();

                    try
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
            if (!_publishStarted)
                return;

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
