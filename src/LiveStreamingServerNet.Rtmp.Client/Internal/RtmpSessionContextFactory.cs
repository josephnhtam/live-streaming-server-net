using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpSessionContextFactory : IRtmpSessionContextFactory
    {
        public IRtmpSessionContext Create(ISessionHandle session)
            => new RtmpSessionContext(session);
    }
}