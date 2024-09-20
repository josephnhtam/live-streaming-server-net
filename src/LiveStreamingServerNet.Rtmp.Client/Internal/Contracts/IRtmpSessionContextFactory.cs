using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpSessionContextFactory
    {
        IRtmpSessionContext Create(ISessionHandle session);
    }
}