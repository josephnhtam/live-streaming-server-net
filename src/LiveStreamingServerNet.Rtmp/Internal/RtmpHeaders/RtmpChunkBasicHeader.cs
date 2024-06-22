using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders
{
    internal record struct RtmpChunkBasicHeader(int ChunkType, uint ChunkStreamId)
    {
        public int Size => ChunkStreamId switch
        {
            <= 63 => 1,
            <= 319 => 2,
            _ => 3
        };

        public static async ValueTask<RtmpChunkBasicHeader> ReadAsync(IDataBuffer dataBuffer, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            await dataBuffer.FromStreamData(networkStream, 1, cancellationToken);

            var firstByte = dataBuffer.ReadByte();
            var chunkStreamIdAttempt = (uint)(firstByte & 0x3f);

            var chunkType = firstByte >> 6;

            switch (chunkStreamIdAttempt)
            {
                case 0:
                    await dataBuffer.FromStreamData(networkStream, 1, cancellationToken);
                    return new RtmpChunkBasicHeader(chunkType, 64u + dataBuffer.ReadByte());
                case 1:
                    await dataBuffer.FromStreamData(networkStream, 2, cancellationToken);
                    return new RtmpChunkBasicHeader(chunkType, 64u + dataBuffer.ReadByte() + 256u * dataBuffer.ReadByte());
                default:
                    return new RtmpChunkBasicHeader(chunkType, chunkStreamIdAttempt);
            }
        }

        public void Write(IDataBuffer dataBuffer)
        {
            var chunkStreamIdAttempt = ChunkStreamId;

            if (ChunkStreamId >= 64 && ChunkStreamId <= 319)
            {
                chunkStreamIdAttempt = 0;
            }
            else if (ChunkStreamId > 319)
            {
                chunkStreamIdAttempt = 1;
            }

            var firstByte = ChunkType << 6 | (byte)(chunkStreamIdAttempt & 0x3f);

            dataBuffer.Write((byte)firstByte);

            switch (chunkStreamIdAttempt)
            {
                case 0:
                    dataBuffer.Write((byte)(ChunkStreamId - 64));
                    break;
                case 1:
                    dataBuffer.Write((byte)((ChunkStreamId - 64) % 256));
                    dataBuffer.Write((byte)((ChunkStreamId - 64) / 256));
                    break;
            }
        }
    }
}
