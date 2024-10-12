namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpDownstreamRelayCondition
    {
        ValueTask<bool> ShouldRelayStreamAsync(IServiceProvider services, string streamPath);
    }
}
