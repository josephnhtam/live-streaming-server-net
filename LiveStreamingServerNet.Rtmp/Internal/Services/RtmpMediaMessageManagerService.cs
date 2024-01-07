using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpMediaMessageManagerService : IRtmpMediaMessageManagerService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly INetBufferPool _netBufferPool;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger<RtmpMediaMessageManagerService> _logger;

        private readonly ConcurrentDictionary<IRtmpClientContext, ClientMediaContext> _clientMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientContext, Task> _clientTasks = new();

        public RtmpMediaMessageManagerService(
            IRtmpChunkMessageSenderService chunkMessageSender,
            INetBufferPool netBufferPool,
            IOptions<MediaMessageConfiguration> config,
            ILogger<RtmpMediaMessageManagerService> logger)
        {
            _chunkMessageSender = chunkMessageSender;
            _netBufferPool = netBufferPool;
            _config = config.Value;
            _logger = logger;
        }

        public void EnqueueAudioMessage(IRtmpClientContext subscriber, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, MediaType.Audio, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void EnqueueAudioMessage(IList<IRtmpClientContext> subscribers, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, MediaType.Audio, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void EnqueueVideoMessage(IRtmpClientContext subscriber, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, MediaType.Video, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void EnqueueVideoMessage(IList<IRtmpClientContext> subscribers, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, MediaType.Video, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void SendCachedHeaderMessages(IRtmpClientContext clientContext, IRtmpPublishStreamContext publishStreamContext, uint timestamp, uint streamId)
        {
            var videoSequenceHeader = publishStreamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                SendMediaPackage(clientContext, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, timestamp, streamId);
            }

            var audioSequenceHeader = publishStreamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                SendMediaPackage(clientContext, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, timestamp, streamId);
            }
        }

        public void SendCachedStreamMetaDataMessage(IRtmpClientContext clientContext, IRtmpPublishStreamContext publishStreamContext, uint timestamp, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

            _chunkMessageSender.Send(clientContext, basicHeader, messageHeader, (netBuffer) =>
                netBuffer.WriteAmf([RtmpDataMessageConstants.OnMetaData, publishStreamContext.StreamMetaData], AmfEncodingType.Amf0)
            );
        }

        public void SendCachedStreamMetaDataMessage(IList<IRtmpClientContext> clientContexts, IRtmpPublishStreamContext publishStreamContext, uint timestamp, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

            _chunkMessageSender.Send(clientContexts, basicHeader, messageHeader, (netBuffer) =>
                netBuffer.WriteAmf([RtmpDataMessageConstants.OnMetaData, publishStreamContext.StreamMetaData], AmfEncodingType.Amf0)
            );
        }

        public void SendCachedGroupOfPictures(IRtmpClientContext clientContext, IRtmpPublishStreamContext publishStreamContext, uint streamId)
        {
            foreach (var picture in publishStreamContext.GroupOfPicturesCache.Get())
            {
                SendMediaPackage(clientContext, picture.Type, picture.Payload.Buffer, picture.PayloadSize, picture.Timestamp, streamId);
                picture.Payload.Unclaim();
            }
        }

        private void EnqueueMediaMessage(IRtmpClientContext subscriber, MediaType mediaType, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            if (!_clientMediaContexts.TryGetValue(subscriber, out var mediaContext))
                return;

            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = new RentedBuffer(netBuffer.Size);
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, netBuffer.Size);

            var mediaPackage = new ClientMediaPackage(
                mediaType,
                timestamp,
                streamId,
                rentedBuffer,
                netBuffer.Size,
                isSkippable);

            mediaContext.AddPackage(ref mediaPackage);
        }

        private void EnqueueMediaMessage(IList<IRtmpClientContext> subscribers, MediaType mediaType, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            subscribers = subscribers.Where(FilterSubscribers).ToList();

            if (!subscribers.Any())
                return;

            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = new RentedBuffer(netBuffer.Size, subscribers.Count);
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, netBuffer.Size);

            var mediaPackage = new ClientMediaPackage(
                mediaType,
                timestamp,
                streamId,
                rentedBuffer,
                netBuffer.Size,
                isSkippable);

            foreach (var subscriber in subscribers)
            {
                if (!_clientMediaContexts.TryGetValue(subscriber, out var mediaContext) || !mediaContext.AddPackage(ref mediaPackage))
                    rentedBuffer.Unclaim();
            }

            bool FilterSubscribers(IRtmpClientContext subscriber)
            {
                var subscriptionContext = subscriber.StreamSubscriptionContext;

                if (subscriptionContext == null)
                    return false;

                switch (mediaType)
                {
                    case MediaType.Audio:
                        if (!subscriptionContext.IsReceivingAudio)
                            return false;
                        break;
                    case MediaType.Video:
                        if (!subscriptionContext.IsReceivingVideo)
                            return false;
                        break;
                }

                return true;
            }
        }

        private void SendMediaPackage(IRtmpClientContext clientContext, MediaType type, byte[] payloadBuffer, int payloadSize, uint timestamp, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(
                    0,
                    type == MediaType.Video ?
                    RtmpConstants.VideoMessageChunkStreamId :
                    RtmpConstants.AudioMessageChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                type == MediaType.Video ?
                RtmpMessageType.VideoMessage :
                RtmpMessageType.AudioMessage,
                streamId);

            _chunkMessageSender.Send(clientContext, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize));
        }

        private async Task SendMediaPackageAsync(IRtmpClientContext clientContext, MediaType type, byte[] payloadBuffer, int payloadSize, uint timestamp, uint streamId, CancellationToken cancellation)
        {
            var basicHeader = new RtmpChunkBasicHeader(
                    0,
                    type == MediaType.Video ?
                    RtmpConstants.VideoMessageChunkStreamId :
                    RtmpConstants.AudioMessageChunkStreamId);

            var messageHeader = new RtmpChunkMessageHeaderType0(
                timestamp,
                type == MediaType.Video ?
                RtmpMessageType.VideoMessage :
                RtmpMessageType.AudioMessage,
                streamId);

            await _chunkMessageSender.SendAsync(clientContext, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize))
                                     .WithCancellation(cancellation);
        }

        public void RegisterClient(IRtmpClientContext clientContext)
        {
            _clientMediaContexts[clientContext] = new ClientMediaContext(clientContext, _config, _logger);

            var clientTask = Task.Run(() => ClientTask(clientContext));
            _clientTasks[clientContext] = clientTask;
            _ = clientTask.ContinueWith(_ => _clientTasks.TryRemove(clientContext, out var _));
        }

        public void UnregisterClient(IRtmpClientContext clientContext)
        {
            if (_clientMediaContexts.TryRemove(clientContext, out var context))
            {
                context.Stop();
            }
        }

        private async Task ClientTask(IRtmpClientContext clientContext)
        {
            var context = _clientMediaContexts[clientContext];
            var cancellation = context.CancellationToken;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var package = await context.ReadPackageAsync(cancellation);

                    try
                    {
                        if (clientContext.StreamSubscriptionContext == null)
                            continue;

                        await clientContext.StreamSubscriptionContext.InitializationTask;

                        await SendMediaPackageAsync(
                            clientContext,
                            package.MediaType,
                            package.RentedPayload.Buffer,
                            package.PayloadSize,
                            package.Timestamp,
                            package.StreamId,
                            cancellation);
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
                    catch (Exception ex)
                    {
                        _logger.FailedToSendMediaMessage(clientContext.Client.ClientId, ex);
                    }
                    finally
                    {
                        package.RentedPayload.Unclaim();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }

            while (context.ReadPackage(out var package))
            {
                package.RentedPayload.Unclaim();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_clientTasks.Values);
        }

        private class ClientMediaContext
        {
            public readonly IRtmpClientContext ClientContext;
            public readonly CancellationToken CancellationToken;
            public long OutstandingPackagesSize => _outstandingPackagesSize;
            public long OutstandingPackagesCount => _packageChannel.Reader.Count;

            private readonly MediaMessageConfiguration _config;
            private readonly ILogger _logger;

            private readonly Channel<ClientMediaPackage> _packageChannel;
            private readonly CancellationTokenSource _cts;

            private long _outstandingPackagesSize;
            private bool _skippingPackage;

            public ClientMediaContext(IRtmpClientContext clientContext, MediaMessageConfiguration config, ILogger logger)
            {
                ClientContext = clientContext;
                _config = config;
                _logger = logger;

                _packageChannel = Channel.CreateUnbounded<ClientMediaPackage>();
                _cts = new CancellationTokenSource();
                CancellationToken = _cts.Token;
            }

            public void Stop()
            {
                _packageChannel.Writer.Complete();
                _cts.Cancel();
            }

            public bool AddPackage(ref ClientMediaPackage package)
            {
                if (ShouldSkipPackage(this, ref package))
                {
                    return false;
                }

                if (!_packageChannel.Writer.TryWrite(package))
                {
                    return false;
                }

                Interlocked.Add(ref _outstandingPackagesSize, package.PayloadSize);
                return true;
            }

            public async ValueTask<ClientMediaPackage> ReadPackageAsync(CancellationToken cancellation)
            {
                var package = await _packageChannel.Reader.ReadAsync(cancellation);
                Interlocked.Add(ref _outstandingPackagesSize, -package.PayloadSize);
                return package;
            }

            public bool ReadPackage(out ClientMediaPackage package)
            {
                return _packageChannel.Reader.TryRead(out package);
            }

            private bool ShouldSkipPackage(ClientMediaContext context, ref ClientMediaPackage package)
            {
                if (!package.IsSkippable)
                {
                    _skippingPackage = false;
                    return false;
                }

                if (_skippingPackage)
                {
                    if (context.OutstandingPackagesSize <= _config.MaxOutstandingMediaMessageSize ||
                        context.OutstandingPackagesCount <= _config.MaxOutstandingMediaMessageCount)
                    {
                        _logger.ResumeMediaPackage(ClientContext.Client.ClientId, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
                        _skippingPackage = false;
                        return false;
                    }

                    return true;
                }

                if (context.OutstandingPackagesSize > _config.MaxOutstandingMediaMessageSize &&
                    context.OutstandingPackagesCount > _config.MaxOutstandingMediaMessageCount)
                {
                    _logger.PauseMediaPackage(ClientContext.Client.ClientId, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
                    _skippingPackage = true;
                    return true;
                }

                return false;
            }
        }

        private record struct ClientMediaPackage(MediaType MediaType, uint Timestamp, uint StreamId, IRentedBuffer RentedPayload, int PayloadSize, bool IsSkippable);
    }
}
