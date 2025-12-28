using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes
{
    [StunAttributeType(IceStunAttributeTypes.ComprehensionOptional.IceControlled)]
    internal record IceControlledAttribute(ulong TieBreaker) : IStunAttribute
    {
        public ushort Type => IceStunAttributeTypes.ComprehensionOptional.IceControlled;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt64BigEndian(TieBreaker);

        public static IceControlledAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new IceControlledAttribute(buffer.ReadUInt64BigEndian());
    }
}
