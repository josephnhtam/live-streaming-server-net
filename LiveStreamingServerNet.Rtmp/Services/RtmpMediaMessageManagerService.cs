using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Utilities;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Services
{
    internal class RtmpMediaMessageManagerService : IRtmpMediaMessageManagerService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly INetBufferPool _netBufferPool;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger<RtmpMediaMessageManagerService> _logger;

        private readonly ConcurrentDictionary<IRtmpClientPeerContext, ClientPeerMediaContext> _peerMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientPeerContext, Task> _peerTasks = new();

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

        public void EnqueueAudioMessage(IRtmpClientPeerContext subscriber, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, MediaType.Audio, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void EnqueueAudioMessage(IList<IRtmpClientPeerContext> subscribers, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, MediaType.Audio, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void EnqueueVideoMessage(IRtmpClientPeerContext subscriber, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, MediaType.Video, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void EnqueueVideoMessage(IList<IRtmpClientPeerContext> subscribers, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, MediaType.Video, timestamp, streamId, isSkippable, payloadWriter);
        }

        public void SendCachedHeaderMessages(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext, uint timestamp, uint streamId)
        {
            var videoSequenceHeader = publishStreamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                SendMediaPackage(peerContext, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, timestamp, streamId);
            }

            var audioSequenceHeader = publishStreamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                SendMediaPackage(peerContext, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, timestamp, streamId);
            }
        }

        public void SendCachedStreamMetaDataMessage(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext, uint timestamp, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

            _chunkMessageSender.Send(peerContext, basicHeader, messageHeader, (netBuffer) =>
                netBuffer.WriteAmf([RtmpDataMessageConstants.OnMetaData, publishStreamContext.StreamMetaData], AmfEncodingType.Amf0)
            );
        }

        public void SendCachedStreamMetaDataMessage(IList<IRtmpClientPeerContext> peerContexts, IRtmpPublishStreamContext publishStreamContext, uint timestamp, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.DataMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp, RtmpMessageType.DataMessageAmf0, streamId);

            _chunkMessageSender.Send(peerContexts, basicHeader, messageHeader, (netBuffer) =>
                netBuffer.WriteAmf([RtmpDataMessageConstants.OnMetaData, publishStreamContext.StreamMetaData], AmfEncodingType.Amf0)
            );
        }

        public void SendCachedGroupOfPictures(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext, uint streamId)
        {
            foreach (var picture in publishStreamContext.GroupOfPicturesCache.Get())
            {
                SendMediaPackage(peerContext, picture.Type, picture.Payload.Buffer, picture.PayloadSize, picture.Timestamp, streamId);
                picture.Payload.Unclaim();
            }
        }

        private void EnqueueMediaMessage(IRtmpClientPeerContext subscriber, MediaType mediaType, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            if (!_peerMediaContexts.TryGetValue(subscriber, out var mediaContext))
                return;

            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = new RentedBuffer(netBuffer.Size);
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, netBuffer.Size);

            var mediaPackage = new ClientPeerMediaPackage(
                mediaType,
                timestamp,
                streamId,
                rentedBuffer,
                netBuffer.Size,
                isSkippable);

            mediaContext.AddPackage(ref mediaPackage);
        }

        private void EnqueueMediaMessage(IList<IRtmpClientPeerContext> subscribers, MediaType mediaType, uint timestamp, uint streamId, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            subscribers = subscribers.Where(FilterSubscribers).ToList();

            if (!subscribers.Any())
                return;

            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = new RentedBuffer(netBuffer.Size, subscribers.Count);
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer.Buffer, 0, netBuffer.Size);

            var mediaPackage = new ClientPeerMediaPackage(
                mediaType,
                timestamp,
                streamId,
                rentedBuffer,
                netBuffer.Size,
                isSkippable);

            foreach (var subscriber in subscribers)
            {
                if (!_peerMediaContexts.TryGetValue(subscriber, out var mediaContext) || !mediaContext.AddPackage(ref mediaPackage))
                    rentedBuffer.Unclaim();
            }

            bool FilterSubscribers(IRtmpClientPeerContext subscriber)
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

        private void SendMediaPackage(IRtmpClientPeerContext peerContext, MediaType type, byte[] payloadBuffer, int payloadSize, uint timestamp, uint streamId)
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

            _chunkMessageSender.Send(peerContext, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize));
        }

        private async Task SendMediaPackageAsync(IRtmpClientPeerContext peerContext, MediaType type, byte[] payloadBuffer, int payloadSize, uint timestamp, uint streamId, CancellationToken cancellation)
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

            await _chunkMessageSender.SendAsync(peerContext, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize))
                                     .WithCancellation(cancellation);
        }

        public void RegisterClientPeer(IRtmpClientPeerContext peerContext)
        {
            _peerMediaContexts[peerContext] = new ClientPeerMediaContext(peerContext, _config, _logger);

            var peerTask = Task.Run(() => ClientPeerTask(peerContext));
            _peerTasks[peerContext] = peerTask;
            _ = peerTask.ContinueWith(_ => _peerTasks.TryRemove(peerContext, out var _));
        }

        public void UnregisterClientPeer(IRtmpClientPeerContext peerContext)
        {
            if (_peerMediaContexts.TryRemove(peerContext, out var context))
            {
                context.Stop();
            }
        }

        private async Task ClientPeerTask(IRtmpClientPeerContext peerContext)
        {
            var context = _peerMediaContexts[peerContext];
            var cancellation = context.CancellationToken;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var package = await context.ReadPackageAsync(cancellation);

                    try
                    {
                        if (peerContext.StreamSubscriptionContext == null)
                            continue;

                        await peerContext.StreamSubscriptionContext.InitializationTask;

                        await SendMediaPackageAsync(
                            peerContext,
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
                        _logger.FailedToSendMediaMessage(peerContext.Peer.PeerId, ex);
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
            await Task.WhenAll(_peerTasks.Values);
        }

        private class ClientPeerMediaContext
        {
            public readonly IRtmpClientPeerContext PeerContext;
            public readonly CancellationToken CancellationToken;
            public long OutstandingPackagesSize => _outstandingPackagesSize;
            public long OutstandingPackagesCount => _packageChannel.Reader.Count;

            private readonly MediaMessageConfiguration _config;
            private readonly ILogger _logger;

            private readonly Channel<ClientPeerMediaPackage> _packageChannel;
            private readonly CancellationTokenSource _cts;

            private long _outstandingPackagesSize;
            private bool _skippingPackage;

            public ClientPeerMediaContext(IRtmpClientPeerContext peerContext, MediaMessageConfiguration config, ILogger logger)
            {
                PeerContext = peerContext;
                _config = config;
                _logger = logger;

                _packageChannel = Channel.CreateUnbounded<ClientPeerMediaPackage>();
                _cts = new CancellationTokenSource();
                CancellationToken = _cts.Token;
            }

            public void Stop()
            {
                _packageChannel.Writer.Complete();
                _cts.Cancel();
            }

            public bool AddPackage(ref ClientPeerMediaPackage package)
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

            public async ValueTask<ClientPeerMediaPackage> ReadPackageAsync(CancellationToken cancellation)
            {
                var package = await _packageChannel.Reader.ReadAsync(cancellation);
                Interlocked.Add(ref _outstandingPackagesSize, -package.PayloadSize);
                return package;
            }

            public bool ReadPackage(out ClientPeerMediaPackage package)
            {
                return _packageChannel.Reader.TryRead(out package);
            }

            private bool ShouldSkipPackage(ClientPeerMediaContext context, ref ClientPeerMediaPackage package)
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
                        _logger.ResumeMediaPackage(PeerContext.Peer.PeerId, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
                        _skippingPackage = false;
                        return false;
                    }

                    return true;
                }

                if (context.OutstandingPackagesSize > _config.MaxOutstandingMediaMessageSize &&
                    context.OutstandingPackagesCount > _config.MaxOutstandingMediaMessageCount)
                {
                    _logger.PauseMediaPackage(PeerContext.Peer.PeerId, context.OutstandingPackagesSize, context.OutstandingPackagesCount);
                    _skippingPackage = true;
                    return true;
                }

                return false;
            }
        }

        private record struct ClientPeerMediaPackage(MediaType MediaType, uint Timestamp, uint StreamId, IRentedBuffer RentedPayload, int PayloadSize, bool IsSkippable);
    }
}
