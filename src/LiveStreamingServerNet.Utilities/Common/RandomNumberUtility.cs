using System.Security.Cryptography;

namespace LiveStreamingServerNet.Utilities.Common
{
    public static class RandomNumberUtility
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public static uint GetRandomUInt32()
        {
            Span<byte> bytes = new byte[sizeof(uint)];
            Random.GetBytes(bytes);
            return BitConverter.ToUInt32(bytes);
        }

        public static ulong GetRandomUInt64()
        {
            Span<byte> bytes = new byte[sizeof(ulong)];
            Random.GetBytes(bytes);
            return BitConverter.ToUInt64(bytes);
        }

        public static void GetRandomBytes(Span<byte> bytes)
        {
            Random.GetBytes(bytes);
        }
    }
}
