using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal record NonceAttribute(string Nonce) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Nonce;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            buffer.WriteUtf8String(Nonce);
        }
    }
}
