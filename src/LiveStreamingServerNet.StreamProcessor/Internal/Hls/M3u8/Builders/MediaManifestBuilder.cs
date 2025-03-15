using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal class MediaManifestBuilder : IMediaManifestBuilder
    {
        private readonly List<MediaSegment> _segments = new();

        private bool _allowCache = false;
        private bool _includeIndependentSegments = true;
        private uint _mediaSequence = 0;
        private TimeSpan? _targetDuration;

        public virtual IMediaManifestBuilder SetTargetDuration(TimeSpan targetDuration)
        {
            _targetDuration = targetDuration;
            return this;
        }

        public virtual IMediaManifestBuilder SetMediaSequence(uint sequenceNumber)
        {
            _mediaSequence = sequenceNumber;
            return this;
        }

        public IMediaManifestBuilder SetAllowCache(bool allowCache)
        {
            _allowCache = allowCache;
            return this;
        }

        public IMediaManifestBuilder SetIndependentSegments(bool includeIndependentSegments)
        {
            _includeIndependentSegments = includeIndependentSegments;
            return this;
        }

        public IMediaManifestBuilder AddSegment(MediaSegment segment)
        {
            _segments.Add(segment);
            return this;
        }

        public string Build()
        {
            var sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");
            sb.AppendLine($"#EXT-X-VERSION:3");
            sb.AppendLine($"#EXT-X-ALLOW-CACHE:{(_allowCache ? "YES" : "NO")}");

            if (_includeIndependentSegments)
            {
                sb.AppendLine("#EXT-X-INDEPENDENT-SEGMENTS");
            }

            if (_targetDuration.HasValue)
            {
                int targetSec = (int)Math.Ceiling(_targetDuration.Value.TotalSeconds);
                sb.AppendLine($"#EXT-X-TARGETDURATION:{targetSec}");
            }

            sb.AppendLine($"#EXT-X-MEDIA-SEQUENCE:{_mediaSequence}");

            foreach (var segment in _segments)
            {
                sb.AppendLine($"#EXTINF:{segment.Duration.TotalSeconds:F1}, {segment.Title ?? ""}");
                sb.AppendLine(segment.Uri);
            }

            return sb.ToString();
        }
    }
}
