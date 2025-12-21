using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionOptional.PasswordAlgorithms)]
    internal record PasswordAlgorithmsAttribute(IList<StunPasswordAlgorithm> Algorithms) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionOptional.PasswordAlgorithms;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            foreach (var algorithm in Algorithms)
            {
                buffer.WriteUInt16BigEndian((ushort)algorithm);
                buffer.WriteUInt16BigEndian(0);
            }
        }

        public static PasswordAlgorithmsAttribute ReadValue(IDataBuffer buffer, ushort length)
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
