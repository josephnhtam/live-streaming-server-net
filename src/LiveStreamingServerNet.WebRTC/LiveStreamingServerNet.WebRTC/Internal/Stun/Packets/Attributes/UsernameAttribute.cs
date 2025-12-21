using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.Username)]
    internal record UsernameAttribute(string Username) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Username;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Username);

        public static UsernameAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new(buffer.ReadUtf8String(length));
    }
}
