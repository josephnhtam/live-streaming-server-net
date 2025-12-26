using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.PasswordAlgorithm)]
    internal record PasswordAlgorithmAttribute(StunPasswordAlgorithm Algorithm) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.PasswordAlgorithm;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            buffer.WriteUInt16BigEndian((ushort)Algorithm);
            buffer.WriteUInt16BigEndian(0);
        }

        public static PasswordAlgorithmAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
        {
            var algorithm = (StunPasswordAlgorithm)buffer.ReadUInt16BigEndian();
            buffer.ReadUInt16BigEndian();
            return new PasswordAlgorithmAttribute(algorithm);
        }
    }
}
