using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionOptional.Fingerprint)]
    internal class FingerprintAttribute : IStunAttribute
    {
        public const ushort Length = 4;
        private const uint XorValue = 0x5354554E;
        private static readonly byte[] Placeholder = new byte[Length];

        private readonly bool _verified;
        public uint Fingerprint { get; }

        public ushort Type => StunAttributeTypes.ComprehensionOptional.Fingerprint;

        public FingerprintAttribute(IDataBuffer buffer)
        {
            _verified = true;

            var tempBuffer = DataBufferPool.Shared.Obtain();
            try
            {
                WritePlaceholder(buffer, tempBuffer);
                Fingerprint = ComputeFingerprint(tempBuffer);
            }
            finally
            {
                DataBufferPool.Shared.Recycle(tempBuffer);
            }
        }

        private void WritePlaceholder(IDataBuffer buffer, IDataBuffer target)
        {
            target.Write(buffer.AsSpan(0, buffer.Position));
            target.WriteUInt16BigEndian(Type);
            target.WriteUInt16BigEndian(Length);
            target.Write(Placeholder);
        }

        private FingerprintAttribute(uint fingerprint, bool verified)
        {
            Fingerprint = fingerprint;
            _verified = verified;
        }

        public bool Verify()
            => _verified == true;

        private static uint ComputeFingerprint(IDataBuffer buffer)
            => CRC32.Generate(buffer.AsSpan(0, buffer.Position)) ^ XorValue;

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.WriteUInt32BigEndian(Fingerprint);

        public static FingerprintAttribute ReadValue(TransactionId transactionId, IDataBuffer buffer, ushort length)
        {
            var tempBuffer = DataBufferPool.Shared.Obtain();

            try
            {
                tempBuffer.Write(buffer.AsSpan(0, buffer.Position));
                tempBuffer.Write(Placeholder);

                var computedFingerprint = ComputeFingerprint(tempBuffer);
                var fingerprint = buffer.ReadUInt32BigEndian();

                return new FingerprintAttribute(fingerprint, computedFingerprint == fingerprint);
            }
            finally
            {
                DataBufferPool.Shared.Recycle(tempBuffer);
            }
        }
    }
}
