using LiveStreamingServerNet.Rtmp.Relay.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts
{
    internal interface IRtmpDownstreamManagerService
    {
        Task<IRtmpDownstreamSubscriber?> RequestDownstreamAsync(string streamPath);
    }
}
