using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders
{
    internal record struct RtmpChunkExtendedTimestampHeader(uint extendedTimestamp)
    {
        public const int kSize = 4;
        public int Size => kSize;

        public static async Task<RtmpChunkExtendedTimestampHeader> ReadAsync(INetBuffer netBuffer, ReadOnlyStream networkStream, CancellationToken cancellationToken)
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
