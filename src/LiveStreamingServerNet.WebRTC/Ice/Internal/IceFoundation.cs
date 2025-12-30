using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal static class IceFoundation
    {
        public static string Create(IceCandidateType type, IPAddress baseAddress)
        {
            var typeStr = type switch
            {
                IceCandidateType.Host => "host",
                IceCandidateType.ServerReflexive => "srflx",
                IceCandidateType.PeerReflexive => "prflx",
                IceCandidateType.Relayed => "relay",
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };

            var familyStr = baseAddress.AddressFamily switch
            {
                AddressFamily.InterNetwork => "v4",
                AddressFamily.InterNetworkV6 => "v6",
                _ => throw new ArgumentOutOfRangeException(nameof(baseAddress.AddressFamily)),
            };

            var maxCharCount = typeStr.Length + familyStr.Length + 2;
            Span<char> charBuffer = stackalloc char[maxCharCount];

            typeStr.CopyTo(charBuffer);
            charBuffer[typeStr.Length] = '|';
            familyStr.CopyTo(charBuffer.Slice(typeStr.Length + 1));
            charBuffer[typeStr.Length + 1 + familyStr.Length] = '|';

            var maxByteCount = Encoding.UTF8.GetMaxByteCount(maxCharCount) + 16;

            Span<byte> byteBuffer = stackalloc byte[maxByteCount];
            var bytesWritten = Encoding.UTF8.GetBytes(charBuffer, byteBuffer);
            baseAddress.TryWriteBytes(byteBuffer.Slice(bytesWritten), out var addressBytesWritten);

            var hash = MD5.HashData(byteBuffer.Slice(0, bytesWritten + addressBytesWritten));
            return BitConverter.ToUInt32(hash, 0).ToString();
        }
    }
}
