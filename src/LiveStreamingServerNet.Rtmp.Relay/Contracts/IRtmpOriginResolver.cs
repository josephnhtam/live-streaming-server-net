namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpOriginResolver
    {
        ValueTask<RtmpOrigin?> ResolveUpstreamOriginAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments, CancellationToken cancellationToken);
        ValueTask<RtmpOrigin?> ResolveDownstreamOriginAsync(string streamPath, CancellationToken cancellationToken);
    }
}
