namespace LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts
{
    public interface IBandwidthLimiterFactory
    {
        IBandwidthLimiter Create();
    }
}
