using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientContextFactory : IRtmpClientContextFactory
    {
        public IRtmpClientContext Create(IClientHandle client)
        {
            return new RtmpClientContext(client);
        }
    }
}
