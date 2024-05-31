using System.Security.Cryptography;

namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class SecurityExtensions
    {
        public static byte[] CalculateHmacSha256(this byte[] bytes, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(bytes);
        }
    }
}