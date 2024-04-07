using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpClientContextFactory
    {
        IRtmpClientContext Create(IClientHandle client);
    }
}
