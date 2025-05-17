using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers
{
    internal class MediaManifestWriter : IMediaManifestWriter
    {
        public async Task WriteAsync(
            string manifestOutputPath,
            IEnumerable<SeqSegment> segments,
            ITargetDuration tagetDuration,
            DateTime? initialProgramDateTime,
            CancellationToken cancellationToken)
        {
            var manifestDirPath = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;

            var manifestBuilder = new MediaManifestBuilder()
                .SetAllowCache(false)
                .SetIndependentSegments(true)
                .SetTargetDuration(tagetDuration.Calculate(segments))
                .SetMediaSequence(segments.FirstOrDefault().SequenceNumber);

            if (initialProgramDateTime.HasValue)
            {
                manifestBuilder = manifestBuilder.SetInitialProgramDateTime(initialProgramDateTime.Value);
            }

            foreach (var segment in segments)
            {
                var relativePath = PathHelper.GetRelativePath(segment.FilePath, manifestDirPath);
                manifestBuilder.AddSegment(new MediaSegment(relativePath, TimeSpan.FromMilliseconds(segment.Timestamp), TimeSpan.FromMilliseconds(segment.Duration)));
            }

            var manifest = manifestBuilder.Build();

            await FileHelper.WriteToFileAsync(manifestOutputPath, manifest);
        }
    }
}
