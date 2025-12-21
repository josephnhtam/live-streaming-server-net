using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttributeType(StunAttributeType.ComprehensionRequired.MessageIntegrity)]
    internal class MessageIntegritySha256Attribute : IStunAttribute
    {
        public const ushort Length = 32;

        private readonly byte[] _hmac;
        public ReadOnlySpan<byte> Hmac => _hmac;

        public ushort Type => StunAttributeType.ComprehensionRequired.MessageIntegritySha256;

        public MessageIntegritySha256Attribute(IDataBuffer buffer, byte[] password)
        {
            _hmac = new byte[Length];
            ComputeHmac(buffer, password, _hmac);
        }

        public MessageIntegritySha256Attribute(byte[] hmac) =>
            _hmac = hmac;

        public bool Verify(IDataBuffer buffer, byte[] password)
        {
            Span<byte> computedHmac = stackalloc byte[Length];

            ComputeHmac(buffer, password, computedHmac);
            return computedHmac.SequenceEqual(_hmac);
        }

        private static void ComputeHmac(IDataBuffer buffer, byte[] password, Span<byte> destination)
        {
            var segment = buffer.AsSegment();

            ReadOnlySpan<byte> data = segment.Array!.AsSpan(segment.Offset, segment.Count);
            HMACSHA256.HashData(password, data, destination);
        }

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
            => buffer.Write(_hmac);

        public static MessageIntegrityAttribute ReadValue(IDataBuffer buffer, ushort length)
            => new(buffer.ReadBytes(length));
    }
}
