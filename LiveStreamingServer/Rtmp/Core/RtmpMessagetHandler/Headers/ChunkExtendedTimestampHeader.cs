using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Extensions;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Headers
{
    public record struct ChunkExtendedTimestampHeader(uint extendedTimestamp)
    {
        public static async Task<ChunkExtendedTimestampHeader> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 4, cancellationToken);

            var extendedTimestamp = netBuffer.ReadUInt32BigEndian();

            return new ChunkExtendedTimestampHeader(extendedTimestamp);
        }
    }
}
