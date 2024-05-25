using LiveStreamingServerNet.Transmuxer.Internal.Containers;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Marshal.Contracts;
using System.Text;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Marshal
{
    internal class ManifestWriter : IManifestWriter
    {
        public async Task WriteAsync(string ManifestOutputPath, IEnumerable<TsSegment> segments, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();

            sb.AppendLine("#EXTM3U");
            sb.AppendLine("#EXT-X-VERSION:3");
            sb.AppendLine("#EXT-X-ALLOW-CACHE:NO");
            sb.AppendLine($"#EXT-X-TARGETDURATION:{CalculateTargetDuration(segments)}");
            sb.AppendLine($"#EXT-X-MEDIA-SEQUENCE:{segments.First().SequenceNumber}");

            foreach (var segment in segments)
            {
                var dirPath = Path.GetDirectoryName(ManifestOutputPath) ?? string.Empty;
                var relativePath = Path.GetRelativePath(dirPath, segment.FilePath).Replace('\\', '/');

                sb.AppendLine($"#EXTINF:{CalculateDuration(segment)},");
                sb.AppendLine(relativePath);
            }

            await File.WriteAllTextAsync(ManifestOutputPath, sb.ToString(), cancellationToken);
        }

        private static string CalculateTargetDuration(IEnumerable<TsSegment> segments)
        {
            return Math.Ceiling(segments.Max(s => s.Duration) / 1000f).ToString();
        }

        private static string CalculateDuration(TsSegment segment)
        {
            return (segment.Duration / 1000f).ToString("n6");
        }
    }
}
