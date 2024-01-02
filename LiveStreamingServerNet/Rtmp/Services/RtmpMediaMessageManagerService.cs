using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Services
{
    public class RtmpMediaMessageManagerService : IRtmpMediaMessageManagerService, IAsyncDisposable
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly INetBufferPool _netBufferPool;

        private readonly ConcurrentDictionary<IRtmpClientPeerContext, ClientPeerMediaContext> _peerMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientPeerContext, Task> _peerTasks = new();

        public RtmpMediaMessageManagerService(IRtmpChunkMessageSenderService chunkMessageSender, INetBufferPool netBufferPool)
        {
            _chunkMessageSender = chunkMessageSender;
            _netBufferPool = netBufferPool;
        }

        private void EnqueueMediaMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, MediaType mediaType, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            if (!_peerMediaContexts.TryGetValue(subscriber, out var mediaContext))
                return;

            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = ArrayPool<byte>.Shared.Rent(netBuffer.Size);
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer, 0, netBuffer.Size);

            var mediaPackage = new ClientPeerMediaPackage(
                mediaType,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId,
                rentedBuffer,
                netBuffer.Size,
                isSkippable);

            if (!mediaContext.PackageChannel.Writer.TryWrite(mediaPackage))
            {
                throw new Exception("Failed to write to the media channel");
            }
        }

        private void EnqueueMediaMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, MediaType mediaType, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            using var netBuffer = _netBufferPool.Obtain();
            payloadWriter(netBuffer);

            var rentedBuffer = ArrayPool<byte>.Shared.Rent(netBuffer.Size);
            netBuffer.MoveTo(0).ReadBytes(rentedBuffer, 0, netBuffer.Size);

            var mediaPackage = new ClientPeerMediaPackage(
                mediaType,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId,
                rentedBuffer,
                netBuffer.Size,
                isSkippable);

            foreach (var subscriber in subscribers)
            {
                if (_peerMediaContexts.TryGetValue(subscriber, out var mediaContext))
                    mediaContext.PackageChannel.Writer.TryWrite(mediaPackage);
            }
        }

        public void SendAudioMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, chunkStreamContext, MediaType.Audio, isSkippable, payloadWriter);
        }

        public void SendAudioMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, chunkStreamContext, MediaType.Audio, isSkippable, payloadWriter);
        }

        public void SendVideoMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, chunkStreamContext, MediaType.Video, isSkippable, payloadWriter);
        }

        public void SendVideoMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, chunkStreamContext, MediaType.Video, isSkippable, payloadWriter);
        }

        private void DoSendAudioMessage(IRtmpClientPeerContext subscriber, uint timestamp, uint messageStreamId, byte[] rentedPayload, int payloadSize)
        {
            try
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.AudioMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(timestamp,
                    RtmpMessageType.AudioMessage, messageStreamId);

                _chunkMessageSender.Send(subscriber, basicHeader, messageHeader,
                    (netBuffer) => netBuffer.Write(rentedPayload, 0, payloadSize));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedPayload);
            }
        }

        private void DoSendVideoMessage(IRtmpClientPeerContext subscriber, uint timestamp, uint messageStreamId, byte[] rentedPayload, int payloadSize)
        {
            try
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.VideoMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(timestamp,
                    RtmpMessageType.VideoMessage, messageStreamId);

                _chunkMessageSender.Send(subscriber, basicHeader, messageHeader,
                    (netBuffer) => netBuffer.Write(rentedPayload, 0, payloadSize));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedPayload);
            }
        }

        public void RegisterClientPeer(IRtmpClientPeerContext peerContext)
        {
            _peerMediaContexts[peerContext] = new ClientPeerMediaContext();

            var peerTask = Task.Run(() => ClientPeerTask(peerContext));
            _peerTasks[peerContext] = peerTask;
            _ = peerTask.ContinueWith(_ => _peerTasks.TryRemove(peerContext, out var _));
        }

        private async Task ClientPeerTask(IRtmpClientPeerContext peerContext)
        {
            var context = _peerMediaContexts[peerContext];
            var packageReader = context.PackageChannel.Reader;
            var cancellation = context.Cts.Token;

            try
            {
                bool debuffering = false;

                await foreach (var package in packageReader.ReadAllAsync(cancellation))
                {
                    if (package.IsSkippable && (packageReader.Count > 10 || debuffering))
                    {
                        if (packageReader.Count < 2)
                            debuffering = false;

                        ArrayPool<byte>.Shared.Return(package.RentedPayload);
                        continue;
                    }

                    switch (package.MediaType)
                    {
                        case MediaType.Video:
                            DoSendVideoMessage(peerContext, package.Timestamp, package.MessageStreamId, package.RentedPayload, package.PayloadSize);
                            break;
                        case MediaType.Audio:
                            DoSendAudioMessage(peerContext, package.Timestamp, package.MessageStreamId, package.RentedPayload, package.PayloadSize);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            when (cancellation.IsCancellationRequested)
            { }
        }

        public void UnregisterClientPeer(IRtmpClientPeerContext peerContext)
        {
            if (_peerMediaContexts.TryRemove(peerContext, out var context))
            {
                context.Cts.Cancel();
                context.PackageChannel.Writer.Complete();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_peerTasks.Values);
        }

        private class ClientPeerMediaContext
        {
            public Channel<ClientPeerMediaPackage> PackageChannel { get; }
            public CancellationTokenSource Cts { get; }

            public ClientPeerMediaContext()
            {
                PackageChannel = Channel.CreateUnbounded<ClientPeerMediaPackage>();
                Cts = new CancellationTokenSource();
            }
        }

        private record struct ClientPeerMediaPackage(MediaType MediaType, uint Timestamp, uint MessageStreamId, byte[] RentedPayload, int PayloadSize, bool IsSkippable);

        private enum MediaType
        {
            Video,
            Audio
        }
    }
}
