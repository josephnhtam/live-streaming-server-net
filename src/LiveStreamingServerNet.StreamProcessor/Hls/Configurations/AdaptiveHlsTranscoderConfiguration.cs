using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Configurations
{
    public class AdaptiveHlsTranscoderConfiguration
    {
        public string Name { get; set; } = "adaptive-hls-transcoder";
        public IStreamProcessorCondition Condition { get; set; } = new DefaultStreamProcessorCondition();
        public IFFmpegOutputPathResolver OutputPathResolver { get; set; } = new DefaultFFmpegOutputPathResolver();

        public string FFmpegPath { get; set; } = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
        public int FFmpegGracefulShutdownTimeoutSeconds { get; set; } = 5;

        public string FFprobePath { get; set; } = ExecutableFinder.FindExecutableFromPATH("ffprobe") ?? string.Empty;
        public int FFprobeGracefulShutdownTimeoutSeconds { get; set; } = 5;

        public PerformanceOptions PerformanceOptions { get; set; } = new(Threads: 1);

        public HlsOptions HlsOptions { get; set; } = new(
            SegmentLength: TimeSpan.FromSeconds(1),
            SegmentListSize: 20,
            DeleteOutdatedSegments: true
        );

        public DownsamplingFilter[] DownsamplingFilters { get; set; } = {
            new DownsamplingFilter("360p", 360, "600k", "64k"),
            new DownsamplingFilter("480p", 480, "1500k", "128k"),
            new DownsamplingFilter("720p", 720, "3000k", "256k"),
        };

        public string VideoEncodingArguments { get; set; } = "-c:v libx264 -preset ultrafast -tune zerolatency -crf 23 -g 30";
        public string AudioEncodingArguments { get; set; } = "-c:a aac";

        public string? VideoDecodingArguments { get; set; }
        public string? AudioDecodingArguments { get; set; }

        public TimeSpan? CleanupDelay { get; set; } = TimeSpan.FromSeconds(30);

        public static Task<string> DefaultOutputPathResolver(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString(), "output.m3u8"));
        }
    }

    public record PerformanceOptions(
        int Threads,
        string? ExtraArguments = null
    );

    public record HlsOptions(
        TimeSpan SegmentLength,
        int SegmentListSize,
        bool DeleteOutdatedSegments,
        IList<string>? Flags = null,
        string? ExtraArguments = null
    );

    public record DownsamplingFilter(string Name, int Height, string MaxVideoBitrate, string MaxAudioBitrate, string? ExtraArguments = null);
}