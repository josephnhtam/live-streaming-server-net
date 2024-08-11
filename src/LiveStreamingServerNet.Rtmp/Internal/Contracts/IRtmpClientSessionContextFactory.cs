using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpClientSessionContextFactory
    {
        IRtmpClientSessionContext Create(ISessionHandle client);
    }
}
