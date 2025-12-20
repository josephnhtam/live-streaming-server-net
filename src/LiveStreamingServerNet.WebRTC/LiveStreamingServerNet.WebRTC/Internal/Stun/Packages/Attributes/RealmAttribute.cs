using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal record RealmAttribute(string Realm) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Realm;

        public void Write(BindingRequest request, IDataBuffer buffer)
        {
            buffer.WriteUtf8String(Realm);
        }
    }
}
