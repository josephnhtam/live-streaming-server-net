using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.RateLimiting
{
    public sealed class BandwidthLimiter : IBandwidthLimiter
    {
        private long _bytesPerSecond;
        private long _bytesLimit;
        private long _permittedBytes;
        private DateTime _lastReplenishmentTime;

        private object _syncLock = new();

        public BandwidthLimiter(long bytesPerSecond, long bytesLimit)
        {
            if (bytesPerSecond < 0) throw new ArgumentOutOfRangeException(nameof(bytesPerSecond));
            if (bytesLimit < 0) throw new ArgumentOutOfRangeException(nameof(bytesLimit));

            _bytesPerSecond = bytesPerSecond;
            _bytesLimit = bytesLimit;
            _permittedBytes = bytesLimit;
            _lastReplenishmentTime = DateTime.UtcNow;
        }

        public bool ConsumeBandwidth(long bytesRequest)
        {
            Debug.Assert(bytesRequest >= 0);

            lock (_syncLock)
            {
                return DoConsumeBandwidth(bytesRequest);
            }
        }

        private bool DoConsumeBandwidth(long bytesRequest)
        {
            var now = DateTime.UtcNow;
            var bytesReplenishment = (long)Math.Ceiling(_bytesPerSecond * (now - _lastReplenishmentTime).TotalSeconds);

            if ((_permittedBytes + bytesReplenishment) < bytesRequest)
                return false;

            _permittedBytes = Math.Min(_permittedBytes + bytesReplenishment - bytesRequest, _bytesLimit);
            _lastReplenishmentTime = now;

            return true;
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}
