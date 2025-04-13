using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts
{
    internal interface IMediaManifestWriter
    {
        Task WriteAsync(
            string manifestOutputPath,
            IEnumerable<SeqSegment> segments,
            ITargetDuration tagetDuration,
            DateTime? initialProgramDateTime,
            CancellationToken cancellationToken = default
        );
    }
}
