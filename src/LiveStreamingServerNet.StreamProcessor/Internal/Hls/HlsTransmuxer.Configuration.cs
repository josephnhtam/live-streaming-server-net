namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls
{
    internal partial class HlsTransmuxer
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string TransmuxerName,
            string ManifestOutputPath,
            string TsFileOutputPath,
            int SegmentListSize,
            bool DeleteOutdatedSegments,
            int MaxSegmentSize,
            int MaxSegmentBufferSize,
            TimeSpan AudioOnlySegmentLength,
            TimeSpan? CleanupDelay
        );
    }
}
