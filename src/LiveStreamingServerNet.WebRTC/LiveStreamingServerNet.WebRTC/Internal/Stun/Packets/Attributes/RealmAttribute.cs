using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttribute(StunAttributeType.ComprehensionRequired.Realm)]
    internal record RealmAttribute(string Realm) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Realm;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            buffer.WriteUtf8String(Realm);
        }
    }
}
