using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.Realm)]
    internal record RealmAttribute(string Realm) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionRequired.Realm;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Realm);

        public static RealmAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
            => new RealmAttribute(buffer.ReadUtf8String(length));
    }
}
