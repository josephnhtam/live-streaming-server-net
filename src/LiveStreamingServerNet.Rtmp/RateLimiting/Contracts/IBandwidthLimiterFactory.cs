namespace LiveStreamingServerNet.Rtmp.RateLimiting.Contracts
{
    public interface IBandwidthLimiterFactory
    {
        IBandwidthLimiter Create();
    }
}
