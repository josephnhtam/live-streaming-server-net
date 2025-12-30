using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace LiveStreamingServerNet.WebRTC.Stun.Internal.Packets.Attributes
{
    [StunAttributeType(StunAttributeTypes.ComprehensionRequired.Userhash)]
    internal class UserhashAttribute : IStunAttribute
    {
        public ushort Type => StunAttributeTypes.ComprehensionRequired.Userhash;

        private readonly byte[] _hash;
        public ReadOnlySpan<byte> Hash => _hash;

        public UserhashAttribute(byte[] hash)
            => _hash = hash;

        public UserhashAttribute(string username, string realm)
            => _hash = ComputeHash(username, realm);

        public bool Verify(string username, string realm)
        {
            var computed = ComputeHash(username, realm);
            return computed.SequenceEqual(_hash);
        }

        private static byte[] ComputeHash(string username, string realm)
        {
            var maxCharCount = username.Length + 1 + realm.Length;
            var maxByteCount = Encoding.UTF8.GetMaxByteCount(maxCharCount);

            Span<char> charBuffer = stackalloc char[maxCharCount];
            username.CopyTo(charBuffer);
            charBuffer[username.Length] = ':';
            realm.CopyTo(charBuffer.Slice(username.Length + 1));

            Span<byte> byteBuffer = stackalloc byte[maxByteCount];
            var bytesWritten = Encoding.UTF8.GetBytes(charBuffer, byteBuffer);

            return SHA256.HashData(byteBuffer.Slice(0, bytesWritten));
        }

        public void WriteValue(TransactionId transactionId, IDataBuffer buffer)
            => buffer.Write(_hash);

        public static UserhashAttribute ReadValue(TransactionId transactionId, IDataBufferReader buffer, ushort length)
            => new UserhashAttribute(buffer.ReadBytes(length));
    }
}
