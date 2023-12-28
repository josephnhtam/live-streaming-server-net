using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;

namespace LiveStreamingServer.Rtmp.Core.RtmpHeaders
{
    public record struct RtmpChunkMessageHeaderType0(uint Timestamp, int MessageLength, byte MessageTypeId, uint MessageStreamId)
    {
        public static async Task<RtmpChunkMessageHeaderType0> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 11, cancellationToken);
            var timestampDelta = netBuffer.ReadUInt24BigEndian();
            var messageLength = (int)netBuffer.ReadUInt24BigEndian();
            var messageTypeId = netBuffer.ReadByte();
            var messageStreamId = netBuffer.ReadUInt32();

            return new RtmpChunkMessageHeaderType0(timestampDelta, messageLength, messageTypeId, messageStreamId);
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt24BigEndian(Timestamp);
            netBuffer.WriteUInt24BigEndian((uint)MessageLength);
            netBuffer.Write(MessageTypeId);
            netBuffer.Write(MessageStreamId);
        }
    }

    public record struct RtmpChunkMessageHeaderType1(uint TimestampDelta, int MessageLength, byte MessageTypeId)
    {
        public static async Task<RtmpChunkMessageHeaderType1> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 7, cancellationToken);

            var timestampDelta = netBuffer.ReadUInt24BigEndian();
            var messageLength = (int)netBuffer.ReadUInt24BigEndian();
            var messageTypeId = netBuffer.ReadByte();

            return new RtmpChunkMessageHeaderType1(timestampDelta, messageLength, messageTypeId);
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt24BigEndian(TimestampDelta);
            netBuffer.WriteUInt24BigEndian((uint)MessageLength);
            netBuffer.Write(MessageTypeId);
        }
    }

    public record struct RtmpChunkMessageHeaderType2(uint TimestampDelta)
    {
        public static async Task<RtmpChunkMessageHeaderType2> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, 3, cancellationToken);

            var timestampDelta = netBuffer.ReadUInt24BigEndian();

            return new RtmpChunkMessageHeaderType2(timestampDelta);
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt24BigEndian(TimestampDelta);
        }
    }
}
