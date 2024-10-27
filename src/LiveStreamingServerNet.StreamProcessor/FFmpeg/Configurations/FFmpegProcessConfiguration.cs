using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations
{
    /// <summary>
    /// Configuration settings for FFmpeg process execution in stream processing.
    /// </summary>
    public class FFmpegProcessConfiguration
    {
        /// <summary>
        /// Gets or sets the name identifier for this FFmpeg process configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path to the FFmpeg executable.
        /// </summary>
        public string FFmpegPath { get; set; }

        /// <summary>
        /// Gets or sets the FFmpeg command line arguments.
        /// Supports placeholders: {inputPath} and {outputPath}.
        /// Default configuration converts input to HLS format with low-latency settings.
        /// </summary>
        public string FFmpegArguments { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds for graceful shutdown of FFmpeg processes.
        /// </summary>
        public int GracefulShutdownTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the resolver for determining FFmpeg output paths.
        /// </summary>
        public IFFmpegOutputPathResolver OutputPathResolver { get; set; }

        /// <summary>
        /// Gets or sets the condition that determines when this FFmpeg configuration should be used.
        /// </summary>
        public IStreamProcessorCondition Condition { get; set; }

        /// <summary>
        /// Initializes a new instance of FFmpegProcessConfiguration with default values.
        /// Default configuration:
        /// - Tries to locate ffmpeg from system PATH
        /// - Uses low-latency HLS output settings
        /// - 5 second graceful shutdown timeout
        /// - Default output path resolver
        /// - Default condition
        /// </summary>
        public FFmpegProcessConfiguration()
        {
            Name = "ffmpeg";
            FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
            FFmpegArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -tune zerolatency -x264-params keyint=15:min-keyint=15 -hls_time 1 -hls_flags delete_segments -hls_list_size 20 -f hls {outputPath}";
            GracefulShutdownTimeoutSeconds = 5;
            OutputPathResolver = new DefaultFFmpegOutputPathResolver();
            Condition = new DefaultStreamProcessorCondition();
        }
    }
}
