namespace LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts
{
    /// <summary>
    /// Creates bandwidth limiter instances.
    /// </summary>
    public interface IBandwidthLimiterFactory
    {
        /// <summary>
        /// Creates a new bandwidth limiter instance.
        /// </summary>
        /// <returns>A bandwidth limiter</returns>
        IBandwidthLimiter Create();
    }
}
