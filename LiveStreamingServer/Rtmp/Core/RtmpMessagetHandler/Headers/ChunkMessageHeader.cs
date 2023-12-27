using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Headers
{
    public record struct ChunkMessageHeaderType0(uint Timestamp, int MessageLength, byte MessageTypeId, uint MessageStreamId)
    {
        public static async Task<ChunkMessageHeaderType0> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 11, cancellationToken);
            var timestampDelta = netBuffer.ReadUInt24BigEndian();
            var messageLength = (int)netBuffer.ReadUInt24BigEndian();
            var messageTypeId = netBuffer.ReadByte();
            var messageStreamId = netBuffer.ReadUInt32();

            return new ChunkMessageHeaderType0(timestampDelta, messageLength, messageTypeId, messageStreamId);
        }
    }

    public record struct ChunkMessageHeaderType1(uint TimestampDelta, int MessageLength, byte MessageTypeId)
    {
        public static async Task<ChunkMessageHeaderType1> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 7, cancellationToken);

            var timestampDelta = netBuffer.ReadUInt24BigEndian();
            var messageLength = (int)netBuffer.ReadUInt24BigEndian();
            var messageTypeId = netBuffer.ReadByte();

            return new ChunkMessageHeaderType1(timestampDelta, messageLength, messageTypeId);
        }
    }

    public record struct ChunkMessageHeaderType2(uint TimestampDelta)
    {
        public static async Task<ChunkMessageHeaderType2> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 3, cancellationToken);

            var timestampDelta = netBuffer.ReadUInt24BigEndian();

            return new ChunkMessageHeaderType2(timestampDelta);
        }
    }
}
