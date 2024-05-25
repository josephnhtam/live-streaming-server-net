namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal partial class HlsTransmuxer
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string Name,
            string ManifestOutputhPath,
            string TsFileOutputPath,
            int SegmentListSize,
            bool DeleteOutdatedSegments
        );
    }
}
