using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionOptional.Fingerprint)]
    internal class FingerprintAttribute : IStunAttribute
    {
        public const ushort Length = 4;
        private const uint XorValue = 0x5354554E;

        public uint Fingerprint { get; }

        public ushort Type => StunAttributeType.ComprehensionOptional.Fingerprint;

        public FingerprintAttribute(IDataBuffer buffer)
            => Fingerprint = ComputeFingerprint(buffer);

        public FingerprintAttribute(uint fingerprint)
            => Fingerprint = fingerprint;

        public bool Verify(IDataBuffer buffer)
            => Fingerprint == ComputeFingerprint(buffer);

        private static uint ComputeFingerprint(IDataBuffer buffer)
            => CRC32.Generate(buffer.AsSpan()) ^ XorValue;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt32BigEndian(Fingerprint);

        public static FingerprintAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
            => new FingerprintAttribute(buffer.ReadUInt32BigEndian());
    }
}
