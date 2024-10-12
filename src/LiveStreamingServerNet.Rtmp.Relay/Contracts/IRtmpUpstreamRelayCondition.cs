namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpUpstreamRelayCondition
    {
        ValueTask<bool> ShouldRelayStreamAsync(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
