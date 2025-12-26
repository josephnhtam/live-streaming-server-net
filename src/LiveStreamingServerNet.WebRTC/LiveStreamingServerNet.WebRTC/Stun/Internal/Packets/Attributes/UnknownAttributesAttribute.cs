using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.UnknownAttributes)]
    internal record UnknownAttributesAttribute(IList<ushort> Types) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.UnknownAttributes;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            foreach (var type in Types)
                buffer.WriteUInt16BigEndian(type);
        }

        public static UnknownAttributesAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
        {
            var types = new List<ushort>();

            for (var i = 0; i < length / 2; i++)
            {
                types.Add(buffer.ReadUInt16BigEndian());
            }

            return new UnknownAttributesAttribute(types);
        }
    }
}
