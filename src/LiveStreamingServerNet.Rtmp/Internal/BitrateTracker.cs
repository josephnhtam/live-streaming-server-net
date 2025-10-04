using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    public class BitrateTracker : IBitrateTracker
    {
        private readonly TimeSpan _trackingWindow;
        private readonly ConcurrentQueue<BytesRecord> _bytesRecords = new();

        public BitrateTracker(TimeSpan trackingWindow)
        {
            if (trackingWindow <= TimeSpan.Zero)
                throw new ArgumentException("Tracking window must be greater than zero.", nameof(trackingWindow));

            _trackingWindow = trackingWindow;
        }

        public void AddBytes(int bytes)
        {
            _bytesRecords.Enqueue(new(bytes, DateTime.UtcNow));

            var cutoffTime = DateTime.UtcNow - _trackingWindow;
            while (_bytesRecords.TryPeek(out var record) && record.Timestamp < cutoffTime)
            {
                _bytesRecords.TryDequeue(out _);
            }
        }

        public int GetBitrateKbps()
        {
            var totalBytes = 0L;
            var earliestTimestamp = DateTime.MaxValue;

            foreach (var record in _bytesRecords)
            {
                totalBytes += record.Bytes;

                if (record.Timestamp < earliestTimestamp)
                    earliestTimestamp = record.Timestamp;
            }

            if (earliestTimestamp == DateTime.MaxValue)
                return 0;

            var timeSpan = DateTime.UtcNow - earliestTimestamp;
            if (timeSpan.TotalSeconds <= 0)
                return 0;

            return (int)((totalBytes * 8) / timeSpan.TotalSeconds / 1000);
        }

        public void Reset()
        {
            _bytesRecords.Clear();
        }

        private record struct BytesRecord(int Bytes, DateTime Timestamp);
    }
}
