using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes.Contracts;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packages.Attributes
{
    internal class MessageIntegritySha256Attribute : IStunAttribute
    {
        public const ushort Length = 32;
        private readonly byte[] _hmac;

        public ushort Type => StunAttributeType.ComprehensionRequired.MessageIntegritySha256;

        public MessageIntegritySha256Attribute(IDataBuffer buffer, byte[] password)
        {
            using var hmac = new HMACSHA256(password);

            var segment = buffer.AsSegment();
            _hmac = hmac.ComputeHash(segment.Array!, segment.Offset, segment.Count);
        }

        public void Write(BindingRequest request, IDataBuffer buffer)
            => buffer.Write(_hmac);
    }
}
