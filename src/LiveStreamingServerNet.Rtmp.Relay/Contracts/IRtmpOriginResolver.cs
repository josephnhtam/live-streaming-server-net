namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpOriginResolver
    {
        ValueTask<RtmpOrigin?> ResolveUpstreamOriginAsync(string streamPath, CancellationToken cancellationToken);
        ValueTask<RtmpOrigin?> ResolveDownstreamOriginAsync(string streamPath, CancellationToken cancellationToken);
    }
}
