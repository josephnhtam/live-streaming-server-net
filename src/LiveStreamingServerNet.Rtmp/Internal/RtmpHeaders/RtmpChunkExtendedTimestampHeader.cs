using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders
{
    internal record struct RtmpChunkExtendedTimestampHeader(uint ExtendedTimestamp)
    {
        public const int kSize = 4;
        public int Size => kSize;

        public static async ValueTask<RtmpChunkExtendedTimestampHeader> ReadAsync(IDataBuffer dataBuffer, INetworkStreamReader networkStream, CancellationToken cancellationToken)
        {
            await dataBuffer.FromStreamData(networkStream, kSize, cancellationToken);

            var extendedTimestamp = dataBuffer.ReadUInt32BigEndian();

            return new RtmpChunkExtendedTimestampHeader(extendedTimestamp);
        }

        public void Write(IDataBuffer dataBuffer)
        {
            dataBuffer.WriteUInt32BigEndian(ExtendedTimestamp);
        }
    }
}
