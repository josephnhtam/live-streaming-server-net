using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding
{
    internal partial class AdaptiveHlsTranscoder
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string Name,
            string ManifestOutputPath,

            string FFmpegPath,
            int FFmpegGracefulTerminationSeconds,

            string FFprobePath,
            int FFprobeGracefulShutdownTimeoutSeconds,

            HlsOptions HlsOptions,
            PerformanceOptions PerformanceOptions,
            IList<DownsamplingFilter> DownsamplingFilters,

            string? VideoEncodingArguments,
            string? AudioEncodingArguments,

            string? VideoDecodingArguments = null,
            string? AudioDecodingArguments = null,

            IList<string>? AdditionalInputs = null,

            TimeSpan? CleanupDelay = null
        );
    }
}
