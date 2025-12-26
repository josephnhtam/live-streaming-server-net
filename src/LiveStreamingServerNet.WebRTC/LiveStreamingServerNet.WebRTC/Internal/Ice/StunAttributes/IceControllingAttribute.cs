using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Ice.StunAttributes
{
    [StunAttributeType(IceStunAttributeType.ComprehensionOptional.IceControlling)]
    internal record IceControllingAttribute(ulong TieBreaker) : IStunAttribute
    {
        public ushort Type => IceStunAttributeType.ComprehensionOptional.IceControlling;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt64BigEndian(TieBreaker);

        public static IceControllingAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new IceControllingAttribute(buffer.ReadUInt64BigEndian());
    }
}
