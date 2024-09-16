using LiveStreamingServerNet.Rtmp.Server.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpServerContext : IRtmpServerContext
    {
        public string AuthCode { get; }

        public RtmpServerContext(IAuthCodeProvider authCodeProvider)
        {
            AuthCode = authCodeProvider.GetAuthCode();
        }
    }
}
