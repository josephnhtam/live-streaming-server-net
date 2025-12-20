using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal class MessageIntegrityAttribute : IStunAttribute
    {
        public const ushort Length = 16;
        private readonly byte[] _hmac;

        public ushort Type => StunAttributeType.ComprehensionRequired.MessageIntegrity;

        public MessageIntegrityAttribute(IDataBuffer buffer, byte[] password)
        {
            using var hmac = new HMACMD5(password);

            var segment = buffer.AsSegment();
            _hmac = hmac.ComputeHash(segment.Array!, segment.Offset, segment.Count);
        }

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
            => buffer.Write(_hmac);
    }
}
