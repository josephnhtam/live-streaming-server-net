using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages
{
    internal readonly record struct BindingRequest
    {
        private const ushort AttributeHeaderLength = 4;

        public TransactionId TransactionId { get; }
        public IReadOnlyList<IStunAttribute> Attributes { get; }

        private static IDataBufferPool _dataBufferPool => DataBufferPool.Shared;

        public BindingRequest(TransactionId transactionId, IList<IStunAttribute> attributes)
        {
            TransactionId = transactionId;
            Attributes = new List<IStunAttribute>(attributes);
        }

        public void Write(IDataBuffer buffer)
        {
            Write(buffer, 0);
        }

        private void Write(IDataBuffer buffer, ushort additionalLength)
        {
            buffer.WriteUInt16BigEndian(StunMessageType.Request);

            var bodyLengthPos = buffer.Position;
            buffer.Advance(2);

            buffer.WriteUInt32BigEndian(StunMessageMagicCookies.Value);
            buffer.Write(TransactionId.Span);

            var (bodyStart, bodyEnd) = WriteAttributes(buffer);
            var bodyLength = bodyEnd - bodyStart;

            buffer.MoveTo(bodyLengthPos);
            buffer.WriteUInt16BigEndian((ushort)(additionalLength + bodyLength));
            buffer.MoveTo(bodyEnd);
        }

        private (int Start, int End) WriteAttributes(IDataBuffer buffer)
        {
            var start = buffer.Position;

            foreach (var attribute in Attributes)
            {
                buffer.WriteUInt16BigEndian(attribute.Type);

                var valueLengthPos = buffer.Position;
                buffer.Advance(2);

                var valueStart = buffer.Position;
                attribute.Write(this, buffer);

                var valueEnd = buffer.Position;
                var valueLength = (ushort)(valueEnd - valueStart);

                buffer.MoveTo(valueLengthPos);
                buffer.WriteUInt16BigEndian(valueLength);
                buffer.MoveTo(valueEnd);

                var paddingBytes = (4 - (valueLength % 4)) % 4;
                for (var i = 0; i < paddingBytes; i++)
                {
                    buffer.Write((byte)0);
                }
            }

            return (start, buffer.Position);
        }


        public BindingRequest WithMessageIntegrity(byte[] password) =>
            WriteSuffixAttribute((original, buffer) =>
            {
                var messageIntegrity = new MessageIntegrityAttribute(buffer, password);
                return new BindingRequest(original.TransactionId, [..original.Attributes, messageIntegrity]);
            }, AttributeHeaderLength + MessageIntegrityAttribute.Length);

        public BindingRequest WithMessageIntegritySha256(byte[] password) =>
            WriteSuffixAttribute((original, buffer) =>
            {
                var messageIntegrity = new MessageIntegritySha256Attribute(buffer, password);
                return new BindingRequest(original.TransactionId, [..original.Attributes, messageIntegrity]);
            }, AttributeHeaderLength + MessageIntegritySha256Attribute.Length);

        public BindingRequest WithFingerprint() =>
            WriteSuffixAttribute(static (original, buffer) =>
            {
                var fingerprint = new FingerprintAttribute(buffer);
                return new BindingRequest(original.TransactionId, [..original.Attributes, fingerprint]);
            }, AttributeHeaderLength + FingerprintAttribute.Length);

        private BindingRequest WriteSuffixAttribute(
            Func<BindingRequest, IDataBuffer, BindingRequest> factory, ushort suffixLength)
        {
            var buffer = _dataBufferPool.Obtain();

            try
            {
                Write(buffer, suffixLength);
                return factory(this, buffer);
            }
            finally
            {
                _dataBufferPool.Recycle(buffer);
            }
        }
    }
}
