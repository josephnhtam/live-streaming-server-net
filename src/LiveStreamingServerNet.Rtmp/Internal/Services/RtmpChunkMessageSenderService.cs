using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpChunkMessageSenderService : IRtmpChunkMessageSenderService
    {
        private readonly INetBufferPool _netBufferPool;

        public RtmpChunkMessageSenderService(INetBufferPool netBufferPool)
        {
            _netBufferPool = netBufferPool;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action? callback) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var extendedTimestampHeader = CreateExtendedTimestampHeader(messageHeader);
            using var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            SendFirstChunk(clientContext, basicHeader, messageHeader, extendedTimestampHeader, callback, payloadBuffer);
            SendRemainingChunks(clientContext, basicHeader, extendedTimestampHeader, callback, payloadBuffer);
        }

        public Task SendAsync<TRtmpChunkMessageHeader>(IRtmpClientContext clientContext, RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var tcs = new TaskCompletionSource();
            Send(clientContext, basicHeader, messageHeader, payloadWriter, tcs.SetResult);
            return tcs.Task;
        }

        public void Send<TRtmpChunkMessageHeader>(
            IList<IRtmpClientContext> clientContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            if (!clientContexts.Any())
                return;

            if (clientContexts.Count == 1)
            {
                Send(clientContexts[0], basicHeader, messageHeader, payloadWriter, null);
                return;
            }

            var extendedTimestampHeader = CreateExtendedTimestampHeader(messageHeader);
            using var payloadBuffer = CreatePayloadBuffer(ref messageHeader, payloadWriter);

            foreach (var clientsGroup in clientContexts.GroupBy(x => x.OutChunkSize))
            {
                var outChunkSize = clientsGroup.Key;
                var clients = clientsGroup.Select(x => x.Client).ToList();

                using var groupPayloadBuffer = _netBufferPool.Obtain();
                payloadBuffer.CopyAllTo(groupPayloadBuffer);
                groupPayloadBuffer.MoveTo(0);

                SendFirstChunk(clients, basicHeader, messageHeader, extendedTimestampHeader, groupPayloadBuffer, outChunkSize);
                SendRemainingChunks(clients, basicHeader, extendedTimestampHeader, groupPayloadBuffer, outChunkSize);
            }
        }

        private void SendFirstChunk<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            Action? callback,
            INetBuffer payloadBuffer)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var outChunkSize = clientContext.OutChunkSize;
            var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;

            clientContext.Client.Send((firstChunkBuffer) =>
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
            IList<IClientHandle> clients,
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

            foreach (var client in clients)
            {
                client.Send(firstChunkBuffer);
            }
        }

        private void SendRemainingChunks(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            Action? callback,
            INetBuffer payloadBuffer)
        {
            var outChunkSize = clientContext.OutChunkSize;

            while (payloadBuffer.Position < payloadBuffer.Size)
            {
                if (!clientContext.Client.IsConnected)
                    return;

                var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;

                clientContext.Client.Send((chunkBuffer) =>
                    WriteToRemainingChunkBuffer(chunkBuffer, basicHeader, extendedTimestampHeader, payloadBuffer, outChunkSize)
                , outChunkSize >= remainingPayloadSize ? callback : null);
            }
        }

        private void SendRemainingChunks(
            IList<IClientHandle> clients,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            INetBuffer payloadBuffer,
            uint outChunkSize)
        {
            while (payloadBuffer.Position < payloadBuffer.Size)
            {
                using var chunkBuffer = _netBufferPool.Obtain();

                WriteToRemainingChunkBuffer(chunkBuffer, basicHeader, extendedTimestampHeader, payloadBuffer, outChunkSize);

                foreach (var client in clients)
                {
                    client.Send(chunkBuffer);
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
            payloadBuffer.ReadAndWriteTo(firstChunkBuffer, payloadSize);
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
            payloadBuffer.ReadAndWriteTo(chunkBuffer, payloadSize);
        }
    }
}
