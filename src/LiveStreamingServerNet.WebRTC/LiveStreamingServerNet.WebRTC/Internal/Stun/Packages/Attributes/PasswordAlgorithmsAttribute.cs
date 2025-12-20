using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal record PasswordAlgorithmsAttribute(IList<StunPasswordAlgorithm> Algorithms) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.PasswordAlgorithms;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            foreach (var algorithm in Algorithms)
            {
                buffer.WriteUInt16BigEndian((ushort)algorithm);
                buffer.WriteUInt16BigEndian(0);
            }
        }
    }

    internal enum StunPasswordAlgorithm : ushort
    {
        MD5 = 0x0001,
        SHA256 = 0x0002
    }
}
