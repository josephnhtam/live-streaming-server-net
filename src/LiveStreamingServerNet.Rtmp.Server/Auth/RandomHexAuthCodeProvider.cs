using LiveStreamingServerNet.Rtmp.Server.Auth.Contracts;
using System.Text;

namespace LiveStreamingServerNet.Rtmp.Server.Auth
{
    /// <summary>
    /// Provides randomly generated hexadecimal auth codes.
    /// </summary>
    public class RandomHexAuthCodeProvider : IAuthCodeProvider
    {
        /// <summary>
        /// Generates a random 64-character hex string for stream access authorization.
        /// </summary>
        /// <returns>A random hex string auth code</returns>
        public string GetAuthCode()
        {
            return GenerateRandomHexString(64);
        }

        /// <summary>
        /// Generates a random hex string of specified length.
        /// The method creates a random alphanumeric string, converts it to bytes,
        /// then formats those bytes as a continuous hex string.
        /// </summary>
        /// <param name="length">The desired length of the initial random string</param>
        /// <returns>A hex string of twice the input length</returns>
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
