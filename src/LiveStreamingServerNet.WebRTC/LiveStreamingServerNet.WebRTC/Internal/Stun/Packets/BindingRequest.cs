using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets
{
    internal class BindingRequest : IDisposable
    {
        private const ushort AttributeHeaderLength = 4;
        private static IDataBufferPool _dataBufferPool => DataBufferPool.Shared;

        public TransactionId TransactionId { get; }
        public IReadOnlyList<IStunAttribute> Attributes => _attributes;

        private readonly List<IStunAttribute> _attributes;

        public BindingRequest(IList<IStunAttribute> attributes)
        {
            TransactionId = TransactionId.Create();
            _attributes = new List<IStunAttribute>(attributes);
        }

        public BindingRequest(TransactionId transactionId, IList<IStunAttribute> attributes)
        {
            transactionId.Claim();

            TransactionId = transactionId;
            _attributes = new List<IStunAttribute>(attributes);
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

            var bodyLength = StunAttributesWriter.Write(buffer, TransactionId, Attributes);
            var bodyEnd = buffer.Position;

            buffer.MoveTo(bodyLengthPos);
            buffer.WriteUInt16BigEndian((ushort)(additionalLength + bodyLength));
            buffer.MoveTo(bodyEnd);
        }

        public BindingRequest WithMessageIntegrity(byte[] password) =>
            WriteSuffixAttribute(buffer =>
            {
                var messageIntegrity = new MessageIntegrityAttribute(buffer, password);
                _attributes.Add(messageIntegrity);
                return this;
            }, AttributeHeaderLength + MessageIntegrityAttribute.Length);

        public BindingRequest WithMessageIntegritySha256(byte[] password) =>
            WriteSuffixAttribute(buffer =>
            {
                var messageIntegrity = new MessageIntegritySha256Attribute(buffer, password);
                _attributes.Add(messageIntegrity);
                return this;
            }, AttributeHeaderLength + MessageIntegritySha256Attribute.Length);

        public BindingRequest WithFingerprint() =>
            WriteSuffixAttribute(buffer =>
            {
                var fingerprint = new FingerprintAttribute(buffer);
                _attributes.Add(fingerprint);
                return this;
            }, AttributeHeaderLength + FingerprintAttribute.Length);

        private BindingRequest WriteSuffixAttribute(
            Func<IDataBuffer, BindingRequest> factory, ushort suffixLength)
        {
            var buffer = _dataBufferPool.Obtain();

            try
            {
                Write(buffer, suffixLength);
                return factory(buffer);
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
