using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8.Contracts;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8
{
    internal class ManifestWriter : IManifestWriter
    {
        public async Task WriteAsync(string manifestOutputPath, IEnumerable<TsSegment> segments, CancellationToken cancellationToken)
        {
            var dirPath = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;
            var tempManifestOutputPath = Path.Combine(dirPath, $"{Path.GetFileName(manifestOutputPath)}.tmp");

            var sb = new StringBuilder();

            sb.AppendLine("#EXTM3U");
            sb.AppendLine("#EXT-X-VERSION:3");
            sb.AppendLine("#EXT-X-ALLOW-CACHE:NO");
            sb.AppendLine("#EXT-X-INDEPENDENT-SEGMENTS");
            sb.AppendLine($"#EXT-X-TARGETDURATION:{CalculateTargetDuration(segments)}");
            sb.AppendLine($"#EXT-X-MEDIA-SEQUENCE:{segments.First().SequenceNumber}");

            foreach (var segment in segments)
            {
                var relativePath = Path.GetRelativePath(dirPath, segment.FilePath).Replace('\\', '/');

                sb.AppendLine($"#EXTINF:{CalculateDuration(segment)},");
                sb.AppendLine(relativePath);
            }

            try
            {
                await File.WriteAllTextAsync(tempManifestOutputPath, sb.ToString(), cancellationToken);
                File.Move(tempManifestOutputPath, manifestOutputPath, true);
            }
            catch
            {
                try
                {
                    File.Delete(tempManifestOutputPath);
                }
                catch { }

                await File.WriteAllTextAsync(manifestOutputPath, sb.ToString(), cancellationToken);
            }
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
