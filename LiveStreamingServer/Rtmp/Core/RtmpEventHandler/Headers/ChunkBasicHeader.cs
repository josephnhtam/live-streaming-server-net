using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Headers
{
    public record struct ChunkBasicHeader(int ChunkType, uint ChunkStreamId)
    {
        public static async Task<ChunkBasicHeader> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 1, cancellationToken);

            var firstByte = netBuffer.ReadByte();
            var chunkStreamIdAttempt = (uint)(firstByte & 0x3f);

            var chunkType = firstByte >> 6;

            switch (chunkStreamIdAttempt)
            {
                case 0:
                    await netBuffer.CopyStreamData(networkStream, 1, cancellationToken);
                    return new ChunkBasicHeader(chunkType, 64u + netBuffer.ReadByte());
                case 1:
                    await netBuffer.CopyStreamData(networkStream, 2, cancellationToken);
                    return new ChunkBasicHeader(chunkType, 64u + netBuffer.ReadByte() + 256u * netBuffer.ReadByte());
                default:
                    return new ChunkBasicHeader(chunkType, chunkStreamIdAttempt);
            }
        }
    }
}
