using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionOptional.PasswordAlgorithms)]
    internal record PasswordAlgorithmsAttribute(IList<StunPasswordAlgorithm> Algorithms) : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionOptional.PasswordAlgorithms;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
        {
            foreach (var algorithm in Algorithms)
            {
                buffer.WriteUInt16BigEndian((ushort)algorithm);
                buffer.WriteUInt16BigEndian(0);
            }
        }

        public static PasswordAlgorithmsAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
        {
            var algorithms = new List<StunPasswordAlgorithm>();

            for (var i = 0; i < length / 4; i++)
            {
                var algorithm = (StunPasswordAlgorithm)buffer.ReadUInt16BigEndian();
                buffer.ReadUInt16BigEndian();

                algorithms.Add(algorithm);
            }

            return new PasswordAlgorithmsAttribute(algorithms);
        }
    }
}
