using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.MessageIntegritySha256)]
    internal class MessageIntegritySha256Attribute : IStunAttribute, IDisposable
    {
        public const ushort Length = HMACSHA256.HashSizeInBytes;
        private static readonly byte[] Placeholder = new byte[Length];

        private readonly byte[] _hmac;
        private readonly IDataBuffer? _cachedBuffer;
        private bool _disposed;

        public ReadOnlySpan<byte> Hmac => _hmac;
        public ushort Type => StunAttributeTypes.ComprehensionRequired.MessageIntegritySha256;

        public MessageIntegritySha256Attribute(IDataBuffer buffer, byte[] password)
        {
            _hmac = new byte[Length];

            _cachedBuffer = DataBufferPool.Shared.Obtain();
            WritePlaceholder(buffer, _cachedBuffer);
            ComputeHmac(_cachedBuffer, password, _hmac);
        }

        private void WritePlaceholder(IDataBuffer buffer, IDataBuffer target)
        {
            target.Write(buffer.AsSpan(0, buffer.Position));
            target.WriteUInt16BigEndian(Type);
            target.WriteUInt16BigEndian(Length);
            target.Write(Placeholder);
        }

        private MessageIntegritySha256Attribute(byte[] hmac, IDataBuffer cachedBuffer)
        {
            _hmac = hmac;
            _cachedBuffer = cachedBuffer;
        }

        public bool Verify(byte[] password)
        {
            if (_cachedBuffer == null)
            {
                return false;
            }

            Span<byte> computedHmac = stackalloc byte[Length];
            ComputeHmac(_cachedBuffer, password, computedHmac);
            return computedHmac.SequenceEqual(_hmac);
        }

        private static void ComputeHmac(IDataBuffer buffer, byte[] password, Span<byte> destination)
        {
            var segment = buffer.AsSegment(0, buffer.Position);

            ReadOnlySpan<byte> data = segment.Array!.AsSpan(segment.Offset, segment.Count);
            HMACSHA256.HashData(password, data, destination);
        }

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.Write(_hmac);

        public static MessageIntegritySha256Attribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
        {
            var cachedBuffer = DataBufferPool.Shared.Obtain();
            cachedBuffer.Write(buffer.AsReadOnlySpan(0, buffer.Position));

            cachedBuffer.MoveTo(2);
            cachedBuffer.WriteUInt16BigEndian((ushort)(buffer.Position + Length - StunMessage.HeaderLength));

            cachedBuffer.MoveTo(buffer.Position);
            cachedBuffer.Write(Placeholder);

            var hmac = buffer.ReadBytes(length);
            return new MessageIntegritySha256Attribute(hmac, cachedBuffer);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_cachedBuffer != null)
            {
                DataBufferPool.Shared.Recycle(_cachedBuffer);
            }
        }
    }
}
