namespace LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts
{
    public interface IBandwidthLimiter : IAsyncDisposable
    {
        bool ConsumeBandwidth(long bytesRequest);
    }
}
