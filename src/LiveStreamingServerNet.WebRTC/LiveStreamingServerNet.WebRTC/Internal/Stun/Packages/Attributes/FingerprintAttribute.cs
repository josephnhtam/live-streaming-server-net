using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal class FingerprintAttribute : IStunAttribute
    {
        public const ushort Length = 4;
        private const uint XorValue = 0x5354554E;

        private readonly uint _fingerprint;

        public ushort Type => StunAttributeType.ComprehensionOptional.Fingerprint;

        public FingerprintAttribute(IDataBuffer buffer)
            => _fingerprint = CRC32.Generate(buffer.AsSpan()) ^ XorValue;

        public void Write(BindingRequest request, IDataBuffer buffer)
            => buffer.WriteUInt32BigEndian(_fingerprint);
    }
}
