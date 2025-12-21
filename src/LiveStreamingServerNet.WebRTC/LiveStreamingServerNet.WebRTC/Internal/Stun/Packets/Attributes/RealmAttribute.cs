using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.Realm)]
    internal record RealmAttribute(string Realm) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Realm;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Realm);

        public static RealmAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new RealmAttribute(buffer.ReadUtf8String(length));
    }
}
