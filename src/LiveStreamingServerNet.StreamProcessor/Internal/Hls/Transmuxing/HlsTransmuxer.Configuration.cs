namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal partial class HlsTransmuxer
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string StreamPath,
            string TransmuxerName,
            string ManifestOutputPath,
            string TsSegmentOutputPath,
            int MaxSegmentSize,
            int MaxSegmentBufferSize,
            TimeSpan MinSegmentLength,
            TimeSpan AudioOnlySegmentLength
        );
    }
}
