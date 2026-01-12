using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes
{
    [StunAttributeType(IceStunAttributeTypes.ComprehensionRequired.Priority)]
    internal record PriorityAttribute(uint Priority) : IStunAttribute
    {
        public ushort Type => IceStunAttributeTypes.ComprehensionRequired.Priority;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt32BigEndian(Priority);

        public static PriorityAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
            => new PriorityAttribute(buffer.ReadUInt32BigEndian());
    }
}
