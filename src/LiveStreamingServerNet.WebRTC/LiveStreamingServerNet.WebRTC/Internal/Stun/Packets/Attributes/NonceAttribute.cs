using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.Nonce)]
    internal record NonceAttribute(string Nonce) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Nonce;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Nonce);

        public static NonceAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new NonceAttribute(buffer.ReadUtf8String(length));
    }
}
