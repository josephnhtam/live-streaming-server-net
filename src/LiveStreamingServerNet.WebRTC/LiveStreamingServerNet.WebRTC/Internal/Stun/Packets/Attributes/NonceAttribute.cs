using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.Nonce)]
    internal record NonceAttribute(string Nonce) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Nonce;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
            => buffer.WriteUtf8String(Nonce);
    }
}
