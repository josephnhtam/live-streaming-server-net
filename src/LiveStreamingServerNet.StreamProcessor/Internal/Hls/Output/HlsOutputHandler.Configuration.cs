namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal partial class HlsOutputHandler
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string StreamPath,
            string TransmuxerName,
            string ManifestOutputPath,
            int SegmentListSize,
            bool DeleteOutdatedSegments,
            TimeSpan? CleanupDelay
        );
    }
}
