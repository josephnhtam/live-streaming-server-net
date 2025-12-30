using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.StunAttributes
{
    [StunAttributeType(IceStunAttributeTypes.ComprehensionOptional.IceControlling)]
    internal record IceControllingAttribute(ulong TieBreaker) : IStunAttribute
    {
        public ushort Type => IceStunAttributeTypes.ComprehensionOptional.IceControlling;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt64BigEndian(TieBreaker);

        public static IceControllingAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
            => new IceControllingAttribute(buffer.ReadUInt64BigEndian());
    }
}
