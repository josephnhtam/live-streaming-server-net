using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using System.Text;

namespace LiveStreamingServerNet.Rtmp.Auth
{
    public class RandomHexAuthCodeProvider : IAuthCodeProvider
    {
        public string GetAuthCode()
        {
            return GenerateRandomHexString(64);
        }

        private static string GenerateRandomHexString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = Random.Shared;
            var randomString = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            var randomBytes = Encoding.UTF8.GetBytes(randomString);
            return BitConverter.ToString(randomBytes).Replace("-", string.Empty);
        }
    }
}
