using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Ice.StunAttributes
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
