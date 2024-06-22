using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpChunkMessageWriterService : IRtmpChunkMessageWriterService
    {
        public void Write<TRtmpChunkMessageHeader>(
            IDataBuffer targetBuffer,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            IDataBuffer payloadBuffer,
            uint outChunkSize) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var extendedTimestampHeader = CreateExtendedTimestampHeader(messageHeader);

            WriteFirstChunk(targetBuffer, basicHeader, messageHeader, extendedTimestampHeader, payloadBuffer, outChunkSize);
            WriteRemainingChunks(targetBuffer, basicHeader, extendedTimestampHeader, payloadBuffer, outChunkSize);
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

        private static void WriteFirstChunk<TRtmpChunkMessageHeader>(
           IDataBuffer targetBuffer,
           RtmpChunkBasicHeader basicHeader,
           TRtmpChunkMessageHeader messageHeader,
           RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
           IDataBuffer payloadBuffer,
           uint outChunkSize) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader
        {
            var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;
            var payloadSize = (int)Math.Min(outChunkSize, remainingPayloadSize);

            basicHeader.Write(targetBuffer);
            messageHeader.Write(targetBuffer);
            extendedTimestampHeader?.Write(targetBuffer);
            payloadBuffer.ReadAndWriteTo(targetBuffer, payloadSize);
        }

        private static void WriteRemainingChunks(
            IDataBuffer targetBuffer,
            RtmpChunkBasicHeader basicHeader,
            RtmpChunkExtendedTimestampHeader? extendedTimestampHeader,
            IDataBuffer payloadBuffer,
            uint outChunkSize)
        {
            while (payloadBuffer.Position < payloadBuffer.Size)
            {
                var remainingPayloadSize = payloadBuffer.Size - payloadBuffer.Position;
                var payloadSize = (int)Math.Min(outChunkSize, remainingPayloadSize);

                var chunkBasicHeader = new RtmpChunkBasicHeader(3, basicHeader.ChunkStreamId);

                chunkBasicHeader.Write(targetBuffer);
                extendedTimestampHeader?.Write(targetBuffer);
                payloadBuffer.ReadAndWriteTo(targetBuffer, payloadSize);
            }
        }
    }
}
