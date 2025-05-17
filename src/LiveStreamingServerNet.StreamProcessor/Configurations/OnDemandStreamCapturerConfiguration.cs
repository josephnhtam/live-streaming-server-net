using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Configurations
{
    public class OnDemandStreamCapturerConfiguration
    {
        /// <summary>
        /// Gets or sets the path to the FFmpeg executable.
        /// Default: ExecutableFinder.FindExecutableFromPATH("ffmpeg").
        /// </summary>
        public string FFmpegPath { get; set; } = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;

        /// <summary>
        /// Gets or sets the timeout in seconds for graceful shutdown of FFmpeg processes.
        /// Default: 5 seconds.
        /// </summary>
        public int FFmpegGracefulTerminationSeconds { get; set; } = 5;
    }
}
