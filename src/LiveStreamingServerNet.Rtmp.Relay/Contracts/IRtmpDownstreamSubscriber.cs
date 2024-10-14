namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpDownstreamSubscriber : IDisposable
    {
        string StreamPath { get; }
    }
}
