using LiveStreamingServerNet.Rtmp.Contracts;
using System.Security.Cryptography;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpServerContext : IRtmpServerContext
    {
        public string AuthCode { get; }

        public RtmpServerContext()
        {
            AuthCode = RandomNumberGenerator.GetHexString(64);
        }
    }
}
