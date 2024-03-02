using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
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
