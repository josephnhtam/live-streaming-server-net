using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttribute(StunAttributeType.ComprehensionOptional.Software)]
    internal record SoftwareAttribute(string Version) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionOptional.Software;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            buffer.WriteUtf8String(Version);
        }
    }
}
