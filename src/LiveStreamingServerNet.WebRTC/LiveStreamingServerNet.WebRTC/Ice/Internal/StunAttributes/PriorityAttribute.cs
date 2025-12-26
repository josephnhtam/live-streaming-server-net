using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes
{
    [StunAttributeType(IceStunAttributeType.ComprehensionRequired.Priority)]
    internal record PriorityAttribute(uint Priority) : IStunAttribute
    {
        public ushort Type => IceStunAttributeType.ComprehensionRequired.Priority;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt32BigEndian(Priority);

        public static PriorityAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new PriorityAttribute(buffer.ReadUInt32BigEndian());
    }
}
