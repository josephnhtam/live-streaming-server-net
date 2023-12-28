using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Services
{
    public class RtmpChunkMessageSenderService : IRtmpChunkMessageSenderService
    {
        private readonly INetBufferPool _netBufferPool;

        public RtmpChunkMessageSenderService(INetBufferPool netBufferPool)
        {
            _netBufferPool = netBufferPool;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action? callback) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var extendedTimestampHeader = CreateExtendedTimestampHeader(messageHeader);

            using var payloadBuffer = _netBufferPool.Obtain();
            payloadWriter.Invoke(payloadBuffer);
            payloadBuffer.MoveTo(0);
            messageHeader.SetMessageLength(payloadBuffer.Size);

            var outChunkSize = (int)peerContext.OutChunkSize;
            SendFirstChunk(peerContext, basicHeader, messageHeader, extendedTimestampHeader, callback, payloadBuffer, outChunkSize);
            SendRemainingChunks(peerContext, basicHeader, extendedTimestampHeader, callback, payloadBuffer, outChunkSize);
        }

        private static RtmpChunkExtendedTimestampHeader? CreateExtendedTimestampHeader<TRtmpChunkMessageHeader>
            (TRtmpChunkMessageHeader messageHeader) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            if (messageHeader.HasExtendedTimestamp())
            {
                var extendedTimestampHeader = new RtmpChunkExtendedTimestampHeader(messageHeader.GetTimestamp());
                messageHeader.UseExtendedTimestamp();
                return extendedTimestampHeader;
            }

            return null;
        }

        private static void SendFirstChunk<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            Action? callback,
            INetBuffer payloadBuffer,
            int outChunkSize) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var headerSize = basicHeader.Size + messageHeader.Size + (extendedTimestampHeader?.Size ?? 0);
            var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;
            var desiredPayloadSize = outChunkSize - headerSize;
            var payloadSize = Math.Min(desiredPayloadSize, remainingPayloadSize);

            peerContext.Peer.Send((firstChunkBuffer) =>
            {
                basicHeader.Write(firstChunkBuffer);
                messageHeader.Write(firstChunkBuffer);
                extendedTimestampHeader?.Write(firstChunkBuffer);

                payloadBuffer.CopyTo(firstChunkBuffer, payloadSize);
            }, desiredPayloadSize >= remainingPayloadSize ? callback : null);
        }

        private static void SendRemainingChunks(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            Action? callback,
            INetBuffer payloadBuffer,
            int outChunkSize)
        {
            if (payloadBuffer.Position >= payloadBuffer.Size)
                return;

            var chunkBasicHeader = new RtmpChunkBasicHeader(3, basicHeader.ChunkStreamId);

            while (payloadBuffer.Position < payloadBuffer.Size)
            {
                var headerSize = chunkBasicHeader.Size + (extendedTimestampHeader?.Size ?? 0);
                var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;
                var desiredPayloadSize = outChunkSize - headerSize;
                var payloadSize = Math.Min(desiredPayloadSize, remainingPayloadSize);

                peerContext.Peer.Send((chunkBuffer) =>
                {
                    chunkBasicHeader.Write(chunkBuffer);
                    extendedTimestampHeader?.Write(chunkBuffer);

                    payloadBuffer.CopyTo(chunkBuffer, payloadSize);
                }, desiredPayloadSize >= remainingPayloadSize ? callback : null);
            }
        }
    }
}
