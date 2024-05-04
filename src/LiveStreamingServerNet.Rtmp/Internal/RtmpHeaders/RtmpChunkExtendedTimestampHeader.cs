using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders
{
    internal record struct RtmpChunkExtendedTimestampHeader(uint ExtendedTimestamp)
    {
        public const int kSize = 4;
        public int Size => kSize;

        public static async ValueTask<RtmpChunkExtendedTimestampHeader> ReadAsync(INetBuffer netBuffer, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.FromStreamData(networkStream, kSize, cancellationToken);

            var extendedTimestamp = netBuffer.ReadUInt32BigEndian();

            return new RtmpChunkExtendedTimestampHeader(extendedTimestamp);
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt32BigEndian(ExtendedTimestamp);
        }
    }
}
