using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Ice.StunAttributes
{
    [StunAttributeType(IceStunAttributeType.ComprehensionOptional.IceControlled)]
    internal record IceControlledAttribute(ulong TieBreaker) : IStunAttribute
    {
        public ushort Type => IceStunAttributeType.ComprehensionOptional.IceControlled;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt64BigEndian(TieBreaker);

        public static IceControlledAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new IceControlledAttribute(buffer.ReadUInt64BigEndian());
    }
}
