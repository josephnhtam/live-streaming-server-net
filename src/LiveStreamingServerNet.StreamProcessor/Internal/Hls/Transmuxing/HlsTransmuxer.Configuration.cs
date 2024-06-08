namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal partial class HlsTransmuxer
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string TransmuxerName,
            string ManifestOutputPath,
            string TsSegmentOutputPath,
            int SegmentListSize,
            bool DeleteOutdatedSegments,
            int MaxSegmentSize,
            int MaxSegmentBufferSize,
            TimeSpan MinSegmentLength,
            TimeSpan AudioOnlySegmentLength,
            TimeSpan? CleanupDelay
        );
    }
}
