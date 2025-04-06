namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal partial class HlsSubtitledOutputHandler
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string StreamPath,
            string TransmuxerName,
            string MasterManifestOutputPath,
            string MediaManifestOutputPath,
            int SegmentListSize,
            bool DeleteOutdatedSegments,
            TimeSpan? CleanupDelay
        );
    }
}
