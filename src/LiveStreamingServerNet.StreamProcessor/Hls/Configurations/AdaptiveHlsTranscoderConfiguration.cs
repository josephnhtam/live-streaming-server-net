using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Hls.Configurations
{
    /// <summary>
    /// Configuration settings for adaptive HLS transcoding operations.
    /// </summary>
    public class AdaptiveHlsTranscoderConfiguration
    {
        /// <summary>
        /// Gets or sets the name identifier for this transcoder configuration.
        /// Default: "adaptive-hls-transcoder".
        /// </summary>
        public string Name { get; set; } = "adaptive-hls-transcoder";

        /// <summary>
        /// Gets or sets the condition that determines when this transcoder should be used.
        /// </summary>
        public IStreamProcessorCondition Condition { get; set; } = new DefaultStreamProcessorCondition();

        /// <summary>
        /// Gets or sets the resolver for determining HLS output paths.
        /// Default Path format: "./output/{contextIdentifier}/output.m3u8".
        /// </summary>
        public IHlsOutputPathResolver OutputPathResolver { get; set; } = new DefaultHlsOutputPathResolver();

        /// <summary>
        /// Gets or sets the path to the FFmpeg executable.
        /// Default: ExecutableFinder.FindExecutableFromPATH("ffmpeg").
        /// </summary>
        public string FFmpegPath { get; set; } = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;

        /// <summary>
        /// Gets or sets the timeout in seconds for graceful shutdown of FFmpeg processes.
        /// Default: 5 seconds.
        /// </summary>
        public int FFmpegGracefulShutdownTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the path to the FFprobe executable.
        /// Default: ExecutableFinder.FindExecutableFromPATH("ffprobe").
        /// </summary>
        public string FFprobePath { get; set; } = ExecutableFinder.FindExecutableFromPATH("ffprobe") ?? string.Empty;

        /// <summary>
        /// Gets or sets the timeout in seconds for graceful shutdown of FFprobe processes.
        /// Default: 5 seconds.
        /// </summary>
        public int FFprobeGracefulShutdownTimeoutSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets performance-related options for transcoding.
        /// Default: 1 thread.
        /// </summary>
        public PerformanceOptions PerformanceOptions { get; set; } = new(Threads: 1);

        /// <summary>
        /// Gets or sets HLS-specific configuration options.
        /// Default: 1 second segments, 20 segments list size, delete outdated segment.
        /// </summary>
        public HlsOptions HlsOptions { get; set; } = new(
            SegmentLength: TimeSpan.FromSeconds(1),
            SegmentListSize: 20,
            DeleteOutdatedSegments: true
        );

        /// <summary>
        /// Gets or sets the array of downsampling filters for adaptive streaming.
        /// Default: 360p (600k/64k), 480p (1500k/128k), 720p (3000k/256k).
        /// </summary>
        public IEnumerable<DownsamplingFilter> DownsamplingFilters { get; set; } = new[] {
            new DownsamplingFilter("360p", 360, "600k", "64k"),
            new DownsamplingFilter("480p", 480, "1500k", "128k"),
            new DownsamplingFilter("720p", 720, "3000k", "256k"),
        };

        /// <summary>
        /// Gets or sets FFmpeg arguments for video encoding.
        /// e.g. "-c:v h264_nvenc -g 30" for hardware-accelerated encoding.
        /// Default: "-c:v libx264 -preset ultrafast -tune zerolatency -crf 23 -g 30".
        /// </summary>
        public string? VideoEncodingArguments { get; set; } = "-c:v libx264 -preset ultrafast -tune zerolatency -crf 23 -g 30";

        /// <summary>
        /// Gets or sets FFmpeg arguments for audio encoding.
        /// Default: "-c:a aac".
        /// </summary>
        public string? AudioEncodingArguments { get; set; } = "-c:a aac";

        /// <summary>
        /// Gets or sets optional FFmpeg arguments for video decoding.
        /// e.g. "-hwaccel auto -c:v h264_cuvid" for hardware-accelerated decoding.
        /// Default: null.
        /// </summary>
        public string? VideoDecodingArguments { get; set; }

        /// <summary>
        /// Gets or sets optional FFmpeg arguments for audio decoding.
        /// Default: null.
        /// </summary>
        public string? AudioDecodingArguments { get; set; }

        /// <summary>
        /// Gets or sets additional video filters to apply.
        /// e.g. ["transpose=1"] for rotating video 90 degrees.
        /// Default: null.
        /// </summary>
        public IEnumerable<string>? VideoFilters { get; set; } = null;

        /// <summary>
        /// Gets or sets additional audio filters to apply.
        /// e.g. ["volume=0.5"] for reducing audio volume by half.
        /// Default: null.
        /// </summary>
        public IEnumerable<string>? AudioFilters { get; set; } = null;

        /// <summary>
        /// Gets or sets additional input paths for FFmpeg.
        /// RTMP stream input is automatically added as the first input.
        /// Default: null.
        /// </summary>
        public IEnumerable<string>? AdditionalInputs { get; set; } = null;

        /// <summary>
        /// Gets or sets additional complex filters to apply.
        /// e.g. ["[1:v]scale=128:128[icon]"] for scaling an overlay image.
        /// Default: null.
        /// </summary>
        public IEnumerable<string>? AdditionalComplexFilters { get; set; } = null;

        /// <summary>
        /// Gets or sets the delay before cleaning up transcoded files.
        /// Default: 30 seconds.
        /// </summary>
        public TimeSpan? CleanupDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Default output path resolver that creates paths in a subdirectory of the current working directory.
        /// Path format: "./output/{contextIdentifier}/output.m3u8".
        /// </summary>
        public static Task<string> DefaultOutputPathResolver(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString(), "output.m3u8"));
        }
    }

    /// <summary>
    /// Performance-related options for transcoding operations.
    /// </summary>
    /// <param name="Threads">Number of threads to use for transcoding. Default: 1.</param>
    /// <param name="MaxMuxingQueueSize">Maximum number of packets in the muxing queue. Default: 1024.</param>
    /// <param name="ExtraArguments">Additional performance-related arguments. Default: null.</param>
    public record PerformanceOptions(
        int Threads,
        int? MaxMuxingQueueSize = 1024,
        string? ExtraArguments = null
    );

    /// <summary>
    /// Configuration options specific to HLS output.
    /// </summary>
    /// <param name="SegmentLength">Duration of each HLS segment. Default: 1 second.</param>
    /// <param name="SegmentListSize">Number of segments to keep in the playlist. Default: 2.0</param>
    /// <param name="DeleteOutdatedSegments">Whether to delete segments not in the playlist. Default: true.</param>
    /// <param name="Flags">Additional HLS flags. Default: null.</param>
    /// <param name="ExtraArguments">Additional HLS-specific arguments. Default: null.</param>
    public record HlsOptions(
        TimeSpan SegmentLength,
        int SegmentListSize,
        bool DeleteOutdatedSegments,
        IEnumerable<string>? Flags = null,
        string? ExtraArguments = null
    );

    /// <summary>
    /// Delegate for generating encoding arguments based on stream index.
    /// </summary>
    public delegate string EncodingArgument(int streamIndex);

    /// <summary>
    /// Defines parameters for video/audio downsampling in adaptive streaming.
    /// </summary>
    /// <param name="Name">Identifier for this filter configuration.</param>
    /// <param name="Height">Target video height in pixels.</param>
    /// <param name="MaxVideoBitrate">Maximum video bitrate (e.g., "600k").</param>
    /// <param name="MaxAudioBitrate">Maximum audio bitrate (e.g., "64k").</param>
    /// <param name="VideoFilter">Additional video filters to apply. Default: null.</param>
    /// <param name="AudioFilter">Additional audio filters to apply. Default: null.</param>
    /// <param name="VideoEncodingArgument">Optional delegate for generating video encoding arguments. Default: null.</param>
    /// <param name="AudioEncodingArgument">Optional delegate for generating audio encoding arguments. Default: null.</param>
    public record DownsamplingFilter(
        string Name,
        int Height,
        string MaxVideoBitrate,
        string MaxAudioBitrate,
        IEnumerable<string>? VideoFilter = null,
        IEnumerable<string>? AudioFilter = null,
        EncodingArgument? VideoEncodingArgument = null,
        EncodingArgument? AudioEncodingArgument = null);
}