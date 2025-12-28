using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.Username)]
    internal record UsernameAttribute(string Username) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionRequired.Username;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Username);

        public static UsernameAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new(buffer.ReadUtf8String(length));
    }
}
