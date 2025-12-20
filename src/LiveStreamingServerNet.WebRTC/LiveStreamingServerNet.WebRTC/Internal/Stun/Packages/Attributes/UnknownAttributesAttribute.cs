using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal record UnknownAttributesAttribute(IList<ushort> Types) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.UnknownAttributes;

        public void Write(BindingRequest request, IDataBuffer buffer)
        {
            foreach (var type in Types)
                buffer.WriteUInt16BigEndian(type);
        }
    }
}
