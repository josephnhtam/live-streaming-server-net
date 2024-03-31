using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;

namespace LiveStreamingServerNet.Rtmp.RateLimiting
{
    public class BandwidthLimiterFactory : IBandwidthLimiterFactory
    {
        private readonly long _bytesPerSecond;
        private readonly long _bytesLimit;

        public BandwidthLimiterFactory(long bytesPerSecond, long bytesLimit)
        {
            _bytesPerSecond = bytesPerSecond;
            _bytesLimit = bytesLimit;
        }

        public IBandwidthLimiter Create()
        {
            return new BandwidthLimiter(_bytesPerSecond, _bytesLimit);
        }
    }
}
