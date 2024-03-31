namespace LiveStreamingServerNet.Rtmp.RateLimiting.Contracts
{
    public interface IBandwidthLimiter : IAsyncDisposable
    {
        bool ConsumeBandwidth(long bytesRequest);
    }
}
