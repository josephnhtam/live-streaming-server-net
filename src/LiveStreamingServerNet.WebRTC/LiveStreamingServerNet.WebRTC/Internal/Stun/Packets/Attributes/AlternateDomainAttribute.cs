using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionOptional.AlternateDomain)]
    internal record AlternateDomainAttribute(string Domain) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionOptional.AlternateDomain;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            buffer.WriteUtf8String(Domain);
        }

        public static AlternateDomainAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new AlternateDomainAttribute(buffer.ReadUtf8String(length));
    }
}
