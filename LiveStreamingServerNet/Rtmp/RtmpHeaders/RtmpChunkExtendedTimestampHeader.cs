using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;

namespace LiveStreamingServerNet.Rtmp.RtmpHeaders
{
    public record struct RtmpChunkExtendedTimestampHeader(uint extendedTimestamp)
    {
        public const int kSize = 4;
        public int Size => kSize;

        public static async Task<RtmpChunkExtendedTimestampHeader> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, kSize, cancellationToken);

            var extendedTimestamp = netBuffer.ReadUInt32BigEndian();

            return new RtmpChunkExtendedTimestampHeader(extendedTimestamp);
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt32BigEndian(extendedTimestamp);
        }
    }
}
