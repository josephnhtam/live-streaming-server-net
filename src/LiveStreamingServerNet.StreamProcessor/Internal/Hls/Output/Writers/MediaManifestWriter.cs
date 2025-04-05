using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers
{
    internal class MediaManifestWriter : IMediaManifestWriter
    {
        public async Task WriteAsync(string manifestOutputPath, IEnumerable<SeqSegment> segments, CancellationToken cancellationToken)
        {
            var manifestDirPath = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;

            var manifestBuilder = new MediaManifestBuilder()
               .SetAllowCache(false)
               .SetIndependentSegments(true)
               .SetTargetDuration(CalculateTargetDuration(segments))
               .SetMediaSequence(segments.FirstOrDefault().SequenceNumber);

            foreach (var segment in segments)
            {
                var relativePath = PathHelper.GetRelativePath(segment.FilePath, manifestDirPath);
                manifestBuilder.AddSegment(new MediaSegment(relativePath, TimeSpan.FromMilliseconds(segment.Timestamp), TimeSpan.FromMilliseconds(segment.Duration)));
            }

            var manifest = manifestBuilder.Build();

            await FileHelper.WriteToFileAsync(manifestOutputPath, manifest);
        }

        private static TimeSpan CalculateTargetDuration(IEnumerable<SeqSegment> segments)
        {
            return TimeSpan.FromMilliseconds(segments.Max(s => s.Duration));
        }
    }
}
