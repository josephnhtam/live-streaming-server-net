using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace LiveStreamingServerNet.Rtmp.Services
{
    public class RtmpMediaMessageManagerService : IRtmpMediaMessageManagerService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly INetBufferPool _netBufferPool;
        private readonly MediaMessageConfiguration _config;

        private readonly ConcurrentDictionary<IRtmpClientPeerContext, ClientPeerMediaContext> _peerMediaContexts = new();
        private readonly ConcurrentDictionary<IRtmpClientPeerContext, Task> _peerTasks = new();

        public RtmpMediaMessageManagerService(IRtmpChunkMessageSenderService chunkMessageSender, INetBufferPool netBufferPool, IOptions<MediaMessageConfiguration> config)
        {
            _chunkMessageSender = chunkMessageSender;
            _netBufferPool = netBufferPool;
            _config = config.Value;
        }

        public void EnqueueAudioMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, chunkStreamContext, MediaType.Audio, isSkippable, payloadWriter);
        }

        public void EnqueueAudioMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, chunkStreamContext, MediaType.Audio, isSkippable, payloadWriter);
        }

        public void EnqueueVideoMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscriber, chunkStreamContext, MediaType.Video, isSkippable, payloadWriter);
        }

        public void EnqueueVideoMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, bool isSkippable, Action<INetBuffer> payloadWriter)
        {
            EnqueueMediaMessage(subscribers, chunkStreamContext, MediaType.Video, isSkippable, payloadWriter);
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

            if (!mediaContext.AddPackage(ref mediaPackage))
                throw new Exception("Failed to write to the media channel");
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
                if (_peerMediaContexts.TryGetValue(subscriber, out var mediaContext))
                    mediaContext.AddPackage(ref mediaPackage);
        }

        private async Task SendAudioMessageAsync(IRtmpClientPeerContext subscriber, uint timestamp, uint messageStreamId, byte[] payloadBuffer, int payloadSize, CancellationToken cancellation)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.AudioMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp,
                RtmpMessageType.AudioMessage, messageStreamId);

            await Task.WhenAny(
                Task.Delay(Timeout.Infinite, cancellation),
                _chunkMessageSender.SendAsync(subscriber, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize))
            );
        }

        private async Task SendVideoMessageAsync(IRtmpClientPeerContext subscriber, uint timestamp, uint messageStreamId, byte[] payloadBuffer, int payloadSize, CancellationToken cancellation)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.VideoMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(timestamp,
                RtmpMessageType.VideoMessage, messageStreamId);

            await Task.WhenAny(
                Task.Delay(Timeout.Infinite, cancellation),
                _chunkMessageSender.SendAsync(subscriber, basicHeader, messageHeader, (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize))
            );
        }

        public void RegisterClientPeer(IRtmpClientPeerContext peerContext)
        {
            _peerMediaContexts[peerContext] = new ClientPeerMediaContext(_config);

            var peerTask = Task.Run(() => ClientPeerTask(peerContext));
            _peerTasks[peerContext] = peerTask;
            _ = peerTask.ContinueWith(_ => _peerTasks.TryRemove(peerContext, out var _));
        }

        public void UnregisterClientPeer(IRtmpClientPeerContext peerContext)
        {
            if (_peerMediaContexts.TryRemove(peerContext, out var context))
            {
                context.Cts.Cancel();
            }
        }

        private async Task ClientPeerTask(IRtmpClientPeerContext peerContext)
        {
            var context = _peerMediaContexts[peerContext];
            var cancellation = context.Cts.Token;

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var package = await context.ReadPackageAsync(cancellation);

                    try
                    {
                        switch (package.MediaType)
                        {
                            case MediaType.Video:
                                await SendVideoMessageAsync(peerContext, package.Timestamp, package.MessageStreamId, package.RentedPayload, package.PayloadSize, cancellation);
                                break;
                            case MediaType.Audio:
                                await SendAudioMessageAsync(peerContext, package.Timestamp, package.MessageStreamId, package.RentedPayload, package.PayloadSize, cancellation);
                                break;
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(package.RentedPayload);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_peerTasks.Values);
        }

        private class ClientPeerMediaContext
        {
            public CancellationTokenSource Cts { get; }
            public long OutstandingPackagesSize => _outstandingPackagesSize;
            public long OutstandingPackagesCount => _packageChannel.Reader.Count;

            private long _outstandingPackagesSize;
            private Channel<ClientPeerMediaPackage> _packageChannel;

            private bool _skippingPackage;
            private readonly MediaMessageConfiguration _config;

            public ClientPeerMediaContext(MediaMessageConfiguration config)
            {
                Cts = new CancellationTokenSource();
                _packageChannel = Channel.CreateUnbounded<ClientPeerMediaPackage>();
                _config = config;
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
                        _skippingPackage = false;
                        return false;
                    }

                    return true;
                }

                if (context.OutstandingPackagesSize > _config.MaxOutstandingMediaMessageSize &&
                    context.OutstandingPackagesCount > _config.MaxOutstandingMediaMessageCount)
                {
                    _skippingPackage = true;
                    return true;
                }

                return false;
            }
        }

        private enum MediaType { Video, Audio }
        private record struct ClientPeerMediaPackage(MediaType MediaType, uint Timestamp, uint MessageStreamId, byte[] RentedPayload, int PayloadSize, bool IsSkippable);
    }
}
