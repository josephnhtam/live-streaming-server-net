using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal static class StunAttributesWriter
    {
        public static int Write(IDataBuffer buffer, TransactionId transactionId, IReadOnlyList<IStunAttribute> attributes)
        {
            var start = buffer.Position;

            foreach (var attribute in attributes)
            {
                buffer.WriteUInt16BigEndian(attribute.Type);

                var valueLengthPos = buffer.Position;
                buffer.Advance(2);

                var valueStart = buffer.Position;
                attribute.WriteValue(transactionId, buffer);

                var valueEnd = buffer.Position;
                var valueLength = (ushort)(valueEnd - valueStart);

                buffer.MoveTo(valueLengthPos);
                buffer.WriteUInt16BigEndian(valueLength);
                buffer.MoveTo(valueEnd);

                var paddingBytes = (4 - (valueLength % 4)) % 4;
                for (var i = 0; i < paddingBytes; i++)
                {
                    buffer.Write((byte)0);
                }
            }

            return buffer.Position - start;
        }
    }
}
