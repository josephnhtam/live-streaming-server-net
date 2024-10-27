namespace LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts
{
    /// <summary>
    /// Controls bandwidth consumption for network traffic.
    /// </summary>
    public interface IBandwidthLimiter : IAsyncDisposable
    {
        /// <summary>
        /// Attempts to consume bandwidth from the available bytes.
        /// </summary>
        /// <param name="bytesRequest">Number of bytes to consume</param>
        /// <returns>True if bandwidth was available and consumed, false if insufficient bandwidth</returns>
        bool ConsumeBandwidth(long bytesRequest);
    }
}
