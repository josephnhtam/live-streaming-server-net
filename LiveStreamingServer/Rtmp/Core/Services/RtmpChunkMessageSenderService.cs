using LiveStreamingServer.Networking.Contracts;
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
            using var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            SendFirstChunk(peerContext, basicHeader, messageHeader, extendedTimestampHeader, callback, payloadBuffer);
            SendRemainingChunks(peerContext, basicHeader, extendedTimestampHeader, callback, payloadBuffer);
        }

        public Task SendAsync<TRtmpChunkMessageHeader>(IRtmpClientPeerContext peerContext, RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new TaskCompletionSource();
            Send(peerContext, basicHeader, messageHeader, payloadWriter, tcs.SetResult);
            return tcs.Task;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IList<IRtmpClientPeerContext> peerContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            if (!peerContexts.Any())
                return;

            if (peerContexts.Count == 1)
            {
                Send(peerContexts[0], basicHeader, messageHeader, payloadWriter, null);
                return;
            }

            var extendedTimestampHeader = CreateExtendedTimestampHeader(messageHeader);
            using var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            foreach (var peersGroup in peerContexts.GroupBy(x => x.OutChunkSize))
            {
                var outChunkSize = peersGroup.Key;
                var peers = peersGroup.Select(x => x.Peer).ToList();

                using var groupPayloadBuffer = _netBufferPool.Obtain();
                payloadBuffer.CopyAllTo(groupPayloadBuffer);

                SendFirstChunk(peers, basicHeader, messageHeader, extendedTimestampHeader, groupPayloadBuffer, outChunkSize);
                SendRemainingChunks(peers, basicHeader, extendedTimestampHeader, groupPayloadBuffer, outChunkSize);
            }
        }

        private void SendFirstChunk<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            Action? callback,
            INetBuffer payloadBuffer)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var outChunkSize = peerContext.OutChunkSize;
            var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;

            peerContext.Peer.Send((firstChunkBuffer) =>
            {
                WriteToFirstChunkBuffer(
                    firstChunkBuffer,
                    basicHeader,
                    messageHeader,
                    extendedTimestampHeader,
                    payloadBuffer,
                    outChunkSize);
            }, outChunkSize >= remainingPayloadSize ? callback : null);
        }

        private void SendFirstChunk<TRtmpChunkMessageHeader>(
            IList<IClientPeerHandle> peers,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            INetBuffer payloadBuffer,
            uint outChunkSize)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            using var firstChunkBuffer = _netBufferPool.Obtain();

            WriteToFirstChunkBuffer(
                firstChunkBuffer,
                basicHeader,
                messageHeader,
                extendedTimestampHeader,
                payloadBuffer,
                outChunkSize);

            foreach (var peer in peers)
            {
                peer.Send(firstChunkBuffer);
            }
        }

        private void SendRemainingChunks(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            Action? callback,
            INetBuffer payloadBuffer)
        {
            var outChunkSize = peerContext.OutChunkSize;

            while (payloadBuffer.Position < payloadBuffer.Size)
            {
                if (!peerContext.Peer.IsConnected)
                    return;

                var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;

                peerContext.Peer.Send((chunkBuffer) =>
                {
                    WriteToRemainingChunkBuffer(chunkBuffer, basicHeader, extendedTimestampHeader, payloadBuffer, outChunkSize);
                }, outChunkSize >= remainingPayloadSize ? callback : null);
            }
        }

        private void SendRemainingChunks(
            IList<IClientPeerHandle> peers,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            INetBuffer payloadBuffer,
            uint outChunkSize)
        {
            while (payloadBuffer.Position < payloadBuffer.Size)
            {
                using var chunkBuffer = _netBufferPool.Obtain();

                WriteToRemainingChunkBuffer(chunkBuffer, basicHeader, extendedTimestampHeader, payloadBuffer, outChunkSize);

                foreach (var peer in peers)
                {
                    peer.Send(chunkBuffer);
                }
            }
        }

        private INetBuffer CreatePayloadBuffer<TRtmpChunkMessageHeader>(ref TRtmpChunkMessageHeader messageHeader, Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var payloadBuffer = _netBufferPool.Obtain();
            payloadWriter.Invoke(payloadBuffer);
            payloadBuffer.MoveTo(0);
            messageHeader.SetMessageLength(payloadBuffer.Size);
            return payloadBuffer;
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

        private void WriteToFirstChunkBuffer<TRtmpChunkMessageHeader>(
            INetBuffer firstChunkBuffer,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            INetBuffer payloadBuffer,
            uint outChunkSize) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;
            var payloadSize = (int)Math.Min(outChunkSize, remainingPayloadSize);

            basicHeader.Write(firstChunkBuffer);
            messageHeader.Write(firstChunkBuffer);
            extendedTimestampHeader?.Write(firstChunkBuffer);
            payloadBuffer.ReadAndCopyTo(firstChunkBuffer, payloadSize);
        }

        private void WriteToRemainingChunkBuffer(
            INetBuffer chunkBuffer,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            INetBuffer payloadBuffer,
            uint outChunkSize)
        {
            var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;
            var payloadSize = (int)Math.Min(outChunkSize, remainingPayloadSize);

            var chunkBasicHeader = new RtmpChunkBasicHeader(3, basicHeader.ChunkStreamId);

            chunkBasicHeader.Write(chunkBuffer);
            extendedTimestampHeader?.Write(chunkBuffer);
            payloadBuffer.ReadAndCopyTo(chunkBuffer, payloadSize);
        }
    }
}
