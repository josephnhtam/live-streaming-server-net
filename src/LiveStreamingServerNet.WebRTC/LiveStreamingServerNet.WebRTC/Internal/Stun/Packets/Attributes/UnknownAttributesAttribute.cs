using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttribute(StunAttributeType.ComprehensionRequired.UnknownAttributes)]
    internal record UnknownAttributesAttribute(IList<ushort> Types) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.UnknownAttributes;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            foreach (var type in Types)
                buffer.WriteUInt16BigEndian(type);
        }
    }
}
