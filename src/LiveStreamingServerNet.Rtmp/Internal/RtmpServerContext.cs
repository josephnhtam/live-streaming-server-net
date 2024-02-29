using LiveStreamingServerNet.Rtmp.Contracts;
using System.Text;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpServerContext : IRtmpServerContext
    {
        public string AuthCode { get; } = GenerateRandomHexString(64);

        private static string GenerateRandomHexString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                           .Select(s => s[random.Next(s.Length)]).ToArray());

            var randomBytes = Encoding.UTF8.GetBytes(randomString);
            return BitConverter.ToString(randomBytes).Replace("-", string.Empty);
        }
    }
}
