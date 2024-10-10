namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    public interface IRtmpOriginResolver
    {
        ValueTask<RtmpOrigin?> ResolveDownstreamOriginAsync(string streamPath, CancellationToken cancellationToken);
    }
}
