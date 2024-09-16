using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Contracts
{
    internal interface IRtmpClientSessionContextFactory
    {
        IRtmpClientSessionContext Create(ISessionHandle client);
    }
}
