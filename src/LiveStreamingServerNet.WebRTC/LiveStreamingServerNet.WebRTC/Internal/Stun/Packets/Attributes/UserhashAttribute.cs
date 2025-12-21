using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace LiveStreamingServerNet.WebRTC.Internal.Stun.Packets.Attributes
{
    [StunAttribute(StunAttributeType.ComprehensionRequired.Userhash)]
    internal record UserhashAttribute(string Username, string Realm) : IStunAttribute
    {
        public ushort Type => StunAttributeType.ComprehensionRequired.Userhash;

        public void WriteValue(BindingRequest request, IDataBuffer buffer)
        {
            using var sha256 = SHA256.Create();

            var input = $"{Username}:{Realm}";
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            buffer.Write(hash);
        }
    }
}
