namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpRelayManager
    {
        Task<IRtmpDownstreamSubscriber?> RequestDownstreamAsync(string streamPath, CancellationToken cancellationToken = default);
        bool IsDownstreamRequested(string streamPath);
    }
}
