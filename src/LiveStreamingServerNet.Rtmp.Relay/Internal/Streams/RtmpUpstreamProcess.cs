using LiveStreamingServerNet.Rtmp.Client;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Exceptions;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Streams
{
    internal class RtmpUpstreamProcess : IRtmpUpstreamProcess
    {
        private readonly string _streamPath;
        private readonly IReadOnlyDictionary<string, string> _streamArguments;
        private readonly IRtmpPublishStreamContext _publishStreamContext;
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly RtmpUpstreamConfiguration _config;
        private readonly ILogger _logger;

        private readonly IPacketDiscarder _packetDiscarder;
        private readonly Channel<StreamData> _streamDataChannel;

        private uint _audioTimestamp;
        private uint _videoTimestamp;
        private long _outstandingPacketsSize;
        private long _outstandingPacketCount;
        private bool _isPublishing;

        public string StreamPath => _streamPath;
        public IReadOnlyDictionary<string, string> StreamArguments => _streamArguments;

        public RtmpUpstreamProcess(
            IRtmpPublishStreamContext publishStreamContext,
            IRtmpOriginResolver originResolver,
            IRtmpStreamManagerService streamManager,
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
            _streamManager = streamManager;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _config = config.Value;
            _logger = logger;

            _packetDiscarder = packetDiscarderFactory.Create(_streamPath);
            _streamDataChannel = CreateStreamDataChannel();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await RunUpstreamClientAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamError(_streamPath, ex);
            }
        }

        private async Task RunUpstreamClientAsync(CancellationToken cancellationToken)
        {
            var retryCounter = new RetryCounter(_config.ReconnectSettings);

            using var abortCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var idleChecker = CreateIdleCheck(abortCts);

            while (!abortCts.IsCancellationRequested)
            {
                try
                {
                    var origin = await ResolveOriginAsync(abortCts.Token);
                    if (origin == null) continue;

                    await DoRunUpstreamClientAsync(origin, idleChecker, retryCounter, abortCts);
                }
                catch (OperationCanceledException) when (abortCts.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.RtmpUpstreamClientError(_streamPath, ex);
                }
                finally
                {
                    await HandleRetryBackoff(retryCounter, abortCts);
                }
            }

            StreamDataChannelCleanup();
            await DisconnectPublisherAsync();

            _logger.RtmpUpstreamStopped(_streamPath);
        }

        private async Task HandleRetryBackoff(IRetryCounter retryCounter, CancellationTokenSource abortCts)
        {
            if (abortCts.IsCancellationRequested)
            {
                return;
            }

            var retryBackoff = retryCounter.GetNextBackoff();

            if (!retryBackoff.HasValue)
            {
                _logger.RtmpUpstreamReconnectLimitReached(_streamPath);
                abortCts.Cancel();
                return;
            }

            try
            {
                _logger.RtmpUpstreamReconnecting(_streamPath, retryBackoff.Value);
                await Task.Delay(retryBackoff.Value, abortCts.Token);
            }
            catch (OperationCanceledException) { }
        }

        private async Task DoRunUpstreamClientAsync(
            RtmpOrigin origin, IIdleChecker idleChecker, IRetryCounter retryCounter, CancellationTokenSource abortCts)
        {
            using var clientCts = CancellationTokenSource.CreateLinkedTokenSource(abortCts.Token);
            using var streamingCts = CancellationTokenSource.CreateLinkedTokenSource(clientCts.Token);
            var initializationTcs = new TaskCompletionSource();

            await using var rtmpClient = CreateUpstreamClient();
            using var _ = clientCts.Token.Register(rtmpClient.Stop);

            _logger.RtmpUpstreamConnecting(_streamPath, origin.EndPoint);
            await rtmpClient.ConnectAsync(origin.EndPoint, origin.AppName);

            _logger.RtmpUpstreamCreating(_streamPath);
            var rtmpStream = await rtmpClient.CreateStreamAsync();

            SubscribeToStreamEvents(rtmpStream, idleChecker, initializationTcs, clientCts);

            var streamDataSendingTask = StreamDataSendingTask(rtmpStream, idleChecker, initializationTcs.Task, streamingCts.Token);
            rtmpStream.Publish.Publish(origin.StreamName);

            _logger.RtmpUpstreamCreated(_streamPath);
            retryCounter.Reset();

            var clientTask = rtmpClient.UntilStoppedAsync(clientCts.Token);
            var completedTask = await Task.WhenAny(clientTask, streamDataSendingTask);

            await HandleCompletedTaskAsync(completedTask, streamDataSendingTask, clientCts);
        }

        private async Task HandleCompletedTaskAsync(Task completedTask, Task streamDataSendingTask, CancellationTokenSource clientCts)
        {
            try
            {
                await completedTask;
            }
            finally
            {
                _isPublishing = false;
                clientCts.Cancel();

                if (completedTask != streamDataSendingTask)
                {
                    try
                    {
                        await streamDataSendingTask;
                    }
                    catch { }
                }
            }
        }

        private async ValueTask<RtmpOrigin?> ResolveOriginAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _originResolver.ResolveUpstreamOriginAsync(_streamPath, _streamArguments, cancellationToken);

                if (result == null)
                {
                    _logger.RtmpUpstreamOriginNotResolved(_streamPath);
                }
                else
                {
                    _logger.RtmpUpstreamOriginResolved(_streamPath, result);
                }

                return result;
            }
            catch
            {
                _logger.RtmpUpstreamOriginNotResolved(_streamPath);
                return null;
            }
        }

        private async Task DisconnectPublisherAsync()
        {
            if (_publishStreamContext.StreamContext == null)
                return;

            try
            {
                await _streamManager.StopPublishingAsync(_publishStreamContext);
                await _publishStreamContext.StreamContext.ClientContext.Client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamError(_streamPath, ex);
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

        private void SubscribeToStreamEvents(IRtmpStream rtmpStream, IIdleChecker idleChecker, TaskCompletionSource initializationTcs, CancellationTokenSource clientCts)
        {
            rtmpStream.OnStatusReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Level == RtmpStatusLevels.Error)
                {
                    clientCts.Cancel();
                }
                else if (eventArgs.Code == RtmpStreamStatusCodes.PublishStart)
                {
                    _isPublishing = true;

                    idleChecker.Refresh();
                    _ = InitializeUpstreamAsync(rtmpStream, initializationTcs, clientCts);
                }
            };
        }

        private async ValueTask InitializeUpstreamAsync(IRtmpStream rtmpStream, TaskCompletionSource initializationTcs, CancellationTokenSource clientCts)
        {
            try
            {
                if (_publishStreamContext.StreamMetaData != null)
                {
                    await SendMetaDataAsync(rtmpStream, _publishStreamContext.StreamMetaData);
                }

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
                            await DoSendMediaDataAsync(rtmpStream, new MediaData(picture.Type, picture.Timestamp, true, picture.Payload));
                        }
                    }
                }
                finally
                {
                    foreach (var picture in pictures)
                        picture.Payload.Unclaim();
                }

                initializationTcs.SetResult();
            }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamInitializationError(_streamPath, ex);
                clientCts.Cancel();
            }

            async ValueTask SendMediaSequenceHeaderAsync(IRtmpStream rtmpStream, MediaType mediaType, byte[] sequenceHeader)
            {
                var buffer = new RentedBuffer(sequenceHeader.Length);

                try
                {
                    sequenceHeader.AsSpan().CopyTo(buffer.Buffer);
                    await DoSendMediaDataAsync(rtmpStream, new MediaData(mediaType, 0, false, buffer));
                }
                finally
                {
                    buffer.Unclaim();
                }
            }
        }

        private async Task SendMetaDataAsync(IRtmpStream rtmpStream, IReadOnlyDictionary<string, object> streamMetaData)
        {
            try
            {
                await rtmpStream.Publish.SendMetaDataAsync(streamMetaData);
            }
            catch (RtmpClientPublishStreamNotAvailableException) { }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamMetaDataSendingError(_streamPath, ex);
                throw;
            }
        }

        private async Task StreamDataSendingTask(
            IRtmpStream rtmpStream, IIdleChecker idleChecker, Task initializationTask, CancellationToken cancellationToken)
        {
            try
            {
                var isInitializationComplete = false;

                while (!cancellationToken.IsCancellationRequested)
                {
                    isInitializationComplete = await UntilInitializationCompleteAsync(isInitializationComplete, initializationTask, cancellationToken);

                    var streamData = await DequeueStreamData(cancellationToken);
                    idleChecker.Refresh();

                    if (streamData.MediaData.HasValue)
                    {
                        await SendMediaDataAsync(rtmpStream, streamData.MediaData.Value);
                    }
                    else if (streamData.MetaData.HasValue)
                    {
                        await SendMetaDataAsync(rtmpStream, streamData.MetaData.Value.StreamMetaData);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _logger.RtmpUpstreamDataSendingError(_streamPath, ex);
                throw;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async ValueTask<bool> UntilInitializationCompleteAsync(
                bool isInitializationComplete, Task initializationTask, CancellationToken cancellationToken)
            {
                if (!isInitializationComplete)
                {
                    await initializationTask.WithCancellation(cancellationToken);
                    isInitializationComplete = true;
                }

                return isInitializationComplete;
            }
        }

        private void StreamDataChannelCleanup()
        {
            _streamDataChannel.Writer.Complete();

            while (_streamDataChannel.Reader.TryRead(out var streamData))
            {
                if (streamData.MediaData.HasValue)
                    streamData.MediaData.Value.Payload.Unclaim();
            }
        }

        private async Task SendMediaDataAsync(IRtmpStream rtmpStream, MediaData mediaData)
        {
            try
            {
                if (!UpdateTimestamp(mediaData.Type, mediaData.Timestamp) && mediaData.IsSkippable)
                    return;

                await DoSendMediaDataAsync(rtmpStream, mediaData);
            }
            catch (RtmpClientPublishStreamNotAvailableException) { }
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

        private static async Task DoSendMediaDataAsync(IRtmpStream rtmpStream, MediaData mediaData)
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

                if (!_streamDataChannel.Writer.TryWrite(new StreamData(mediaData)))
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
        private void EnqueueMetaData(MetaData metaData)
        {
            _streamDataChannel.Writer.TryWrite(new StreamData(metaData));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<StreamData> DequeueStreamData(CancellationToken cancellationToken)
        {
            var streamData = await _streamDataChannel.Reader.ReadAsync(cancellationToken);

            if (streamData.MediaData.HasValue)
            {
                Interlocked.Add(ref _outstandingPacketsSize, -streamData.MediaData.Value.Payload.Size);
                Interlocked.Decrement(ref _outstandingPacketCount);
            }

            return streamData;
        }

        public void OnReceiveMediaData(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            if (!_isPublishing)
                return;

            EnqueueMediaData(new MediaData(mediaType, timestamp, isSkippable, rentedBuffer));
        }

        public void OnReceiveMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            _publishStreamContext.StreamMetaData = metaData;

            if (!_isPublishing)
                return;

            EnqueueMetaData(new MetaData(metaData));
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

        private static Channel<StreamData> CreateStreamDataChannel()
        {
            return Channel.CreateUnbounded<StreamData>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        private record struct StreamData(MediaData? MediaData, MetaData? MetaData)
        {
            public StreamData(MediaData mediaData) : this(mediaData, null) { }
            public StreamData(MetaData metaData) : this(null, metaData) { }
        }

        private record struct MediaData(MediaType Type, uint Timestamp, bool IsSkippable, IRentedBuffer Payload);
        private record struct MetaData(IReadOnlyDictionary<string, object> StreamMetaData);
    }
}
