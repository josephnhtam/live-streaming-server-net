using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class BitrateTrackingService : IBitrateTrackingService
    {
        private readonly ConcurrentDictionary<string, StreamBitrateData> _streamData = new();
        private readonly TimeSpan _trackingWindow = TimeSpan.FromSeconds(10);

        public int GetCurrentVideoBitrate(string streamPath)
        {
            if (!_streamData.TryGetValue(streamPath, out var data))
                return 0;

            return data.GetCurrentVideoBitrate(_trackingWindow);
        }

        public int GetCurrentAudioBitrate(string streamPath)
        {
            if (!_streamData.TryGetValue(streamPath, out var data))
                return 0;

            return data.GetCurrentAudioBitrate(_trackingWindow);
        }

        public void RecordDataReceived(string streamPath, MediaType mediaType, int byteCount)
        {
            var data = _streamData.GetOrAdd(streamPath, _ => new StreamBitrateData());
            data.RecordData(mediaType, byteCount, DateTime.UtcNow, _trackingWindow);
        }

        public void CleanupStream(string streamPath)
        {
            _streamData.TryRemove(streamPath, out _);
        }

        private class StreamBitrateData
        {
            private readonly List<DataPoint> _videoData = new();
            private readonly List<DataPoint> _audioData = new();
            private readonly object _lock = new();

            public void RecordData(MediaType mediaType, int byteCount, DateTime timestamp, TimeSpan cutoffWindow)
            {
                lock (_lock)
                {
                    var dataPoint = new DataPoint(byteCount, timestamp);
                    var cutoffTime = timestamp.Subtract(cutoffWindow);

                    switch (mediaType)
                    {
                        case MediaType.Video:
                            _videoData.Add(dataPoint);
                            CleanupOldData(_videoData, cutoffTime);
                            break;
                        case MediaType.Audio:
                            _audioData.Add(dataPoint);
                            CleanupOldData(_audioData, cutoffTime);
                            break;
                    }
                }
            }

            public int GetCurrentVideoBitrate(TimeSpan window)
            {
                lock (_lock)
                {
                    return CalculateBitrate(_videoData, window);
                }
            }

            public int GetCurrentAudioBitrate(TimeSpan window)
            {
                lock (_lock)
                {
                    return CalculateBitrate(_audioData, window);
                }
            }

            private void CleanupOldData(List<DataPoint> data, DateTime cutoffTime)
            {
                data.RemoveAll(d => d.Timestamp < cutoffTime);
            }

            private int CalculateBitrate(List<DataPoint> data, TimeSpan window)
            {
                if (data.Count == 0)
                    return 0;

                var now = DateTime.UtcNow;
                var cutoffTime = now.Subtract(window);

                var recentData = data.Where(d => d.Timestamp >= cutoffTime).ToList();

                if (recentData.Count < 2)
                    return 0;

                var totalBytes = recentData.Sum(d => d.ByteCount);
                var timeSpan = now - recentData.Min(d => d.Timestamp);

                if (timeSpan.TotalSeconds == 0)
                    return 0;

                return (int)(totalBytes * 8 / timeSpan.TotalSeconds);
            }

            private record struct DataPoint(int ByteCount, DateTime Timestamp);
        }
    }
}
