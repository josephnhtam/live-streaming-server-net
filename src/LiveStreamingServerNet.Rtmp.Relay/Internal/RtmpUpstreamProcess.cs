using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal
{
    internal class RtmpUpstreamProcess : IRtmpUpstreamProcess
    {
        private readonly string _streamPath;
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly RtmpUpstreamConfiguration _config;
        private readonly ILogger _logger;

        private readonly Channel<MediaData> _mediaDataChannel;
        private bool _publishStarted;

        public RtmpUpstreamProcess(
            string streamPath,
            IRtmpOriginResolver originResolver,
            IBufferPool bufferPool,
            IDataBufferPool dataBufferPool,
            IOptions<RtmpUpstreamConfiguration> config,
            ILogger<RtmpUpstreamProcess> logger)
        {
            _streamPath = streamPath;
            _originResolver = originResolver;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config.Value;
            _logger = logger;

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

            var origin = await _originResolver.ResolveUpstreamOriginAsync(_streamPath, abortCts.Token);

            if (origin == null)
            {
                return;
            }

            _logger.RtmpUpstreamOriginResolved(_streamPath, origin.EndPoint);

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
                configurator.Services.AddSingleton(_dataBufferPool);
                configurator.Services.AddSingleton(_bufferPool);

                if (_config.ConfigureUpstreamClient != null)
                    _config.ConfigureUpstreamClient(configurator);
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
                await foreach (var mediaData in _mediaDataChannel.Reader.ReadAllAsync(abortCts.Token))
                {
                    try
                    {
                        idleChecker.Refresh();

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

        private void EnqueueMediaData(MediaData mediaData)
        {
            try
            {
                mediaData.Payload.Claim();

                if (!_mediaDataChannel.Writer.TryWrite(mediaData))
                {
                    throw new ChannelClosedException();
                }
            }
            catch
            {
                mediaData.Payload.Unclaim();
            }
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
