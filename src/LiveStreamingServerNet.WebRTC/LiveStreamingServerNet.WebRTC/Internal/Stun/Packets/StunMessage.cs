using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal class StunMessage : IDisposable
    {
        private readonly StunMessageType _type;
        private const ushort AttributeHeaderLength = 4;
        private static IDataBufferPool _dataBufferPool => DataBufferPool.Shared;

        public TransactionId TransactionId { get; }
        public IReadOnlyList<IStunAttribute> Attributes => _attributes;

        private readonly List<IStunAttribute> _attributes;

        public StunMessage(StunMessageType type, IList<IStunAttribute> attributes)
        {
            TransactionId = TransactionId.Create();
            _type = type;
            _attributes = new List<IStunAttribute>(attributes);
        }

        public StunMessage(TransactionId transactionId, StunMessageType type, IList<IStunAttribute> attributes)
        {
            transactionId.Claim();

            TransactionId = transactionId;
            _type = type;
            _attributes = new List<IStunAttribute>(attributes);
        }

        public static (StunMessage, UnknownAttributes? unknownAttributes) Read(IDataBuffer buffer)
        {
            var type = (StunMessageType)buffer.ReadUInt16BigEndian();
            var bodyLength = buffer.ReadUInt16BigEndian();

            var magicCookie = buffer.ReadUInt32BigEndian();
            if (magicCookie != StunMessageMagicCookies.Value)
            {
                throw new InvalidDataException("Invalid STUN magic cookie.");
            }

            var transactionId = TransactionId.Read(buffer);

            try
            {
                var (attributes, unknownAttributes) = StunAttributesSerializer.Read(buffer, transactionId, bodyLength);
                return (new StunMessage(transactionId, type, attributes), unknownAttributes);
            }
            finally
            {
                transactionId.Unclaim();
            }
        }

        public void Write(IDataBuffer buffer)
        {
            Write(buffer, 0);
        }

        private void Write(IDataBuffer buffer, ushort additionalLength)
        {
            buffer.WriteUInt16BigEndian((ushort)_type);

            var bodyLengthPos = buffer.Position;
            buffer.Advance(2);

            buffer.WriteUInt32BigEndian(StunMessageMagicCookies.Value);
            buffer.Write(TransactionId.Span);

            var bodyLength = StunAttributesSerializer.Write(buffer, TransactionId, Attributes);
            var bodyEnd = buffer.Position;

            buffer.MoveTo(bodyLengthPos);
            buffer.WriteUInt16BigEndian((ushort)(additionalLength + bodyLength));
            buffer.MoveTo(bodyEnd);
        }

        public StunMessage WithMessageIntegrity(byte[] password) =>
            WriteSuffixAttribute(
                buffer => new MessageIntegrityAttribute(buffer, password),
                AttributeHeaderLength + MessageIntegrityAttribute.Length);

        public StunMessage WithMessageIntegritySha256(byte[] password) =>
            WriteSuffixAttribute(
                buffer => new MessageIntegritySha256Attribute(buffer, password),
                AttributeHeaderLength + MessageIntegritySha256Attribute.Length);

        public StunMessage WithFingerprint() =>
            WriteSuffixAttribute(
                buffer => new FingerprintAttribute(buffer),
                AttributeHeaderLength + FingerprintAttribute.Length);

        private StunMessage WriteSuffixAttribute(
            Func<IDataBuffer, IStunAttribute> factory, ushort suffixLength)
        {
            var buffer = _dataBufferPool.Obtain();

            try
            {
                Write(buffer, suffixLength);
                var attribute = factory(buffer);
                _attributes.Add(attribute);
                return this;
            }
            finally
            {
                _dataBufferPool.Recycle(buffer);
            }
        }

        public void Dispose()
        {
            TransactionId.Unclaim();

            foreach (var attribute in Attributes)
            {
                if (attribute is IDisposable disposableAttribute)
                {
                    disposableAttribute.Dispose();
                }
            }
        }
    }
}
