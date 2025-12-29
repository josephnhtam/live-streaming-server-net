using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.Nonce)]
    internal record NonceAttribute(string Nonce) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionRequired.Nonce;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Nonce);

        public static NonceAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
            => new NonceAttribute(buffer.ReadUtf8String(length));
    }
}
