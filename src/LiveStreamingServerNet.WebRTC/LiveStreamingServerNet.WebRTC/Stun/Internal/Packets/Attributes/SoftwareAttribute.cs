using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionOptional.Software)]
    internal record SoftwareAttribute(string Version) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionOptional.Software;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUtf8String(Version);

        public static SoftwareAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
            => new SoftwareAttribute(buffer.ReadUtf8String(length));
    }
}
