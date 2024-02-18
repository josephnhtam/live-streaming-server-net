namespace LiveStreamingServerNet.Rtmp.RateLimiting.Contracts
{
    public interface IBandwidthLimiter
    {
        bool ConsumeBandwidth(long bytesRequest);
    }
}
