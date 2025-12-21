using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionOptional.Software)]
    internal record SoftwareAttribute(string Version) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionOptional.Software;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Version);

        public static SoftwareAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new SoftwareAttribute(buffer.ReadUtf8String(length));
    }
}
