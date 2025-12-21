using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionOptional.AlternateDomain)]
    internal record AlternateDomainAttribute(string Domain) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionOptional.AlternateDomain;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            buffer.WriteUtf8String(Domain);
        }

        public static AlternateDomainAttribute ReadValue(IDataBuffer buffer, ushort length)
            => new AlternateDomainAttribute(buffer.ReadUtf8String(length));
    }
}
