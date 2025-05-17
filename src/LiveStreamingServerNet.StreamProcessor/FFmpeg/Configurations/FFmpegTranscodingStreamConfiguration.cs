namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations
{
    /// <summary>
    /// Represents configuration settings for an FFmpeg transcoding stream.
    /// </summary>
    public class FFmpegTranscodingStreamConfiguration
    {
        /// <summary>
        /// Gets the path to the FFmpeg executable.
        /// </summary>
        public string FFmpegPath { get; }

        /// <summary>
        /// Gets the command line arguments for FFmpeg used during transcoding.
        /// </summary>
        public string FFmpegArguments { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FFmpegTranscodingStreamConfiguration"/> class with the specified executable path and arguments.
        /// </summary>
        /// <param name="ffmpegPath">The path to the FFmpeg executable.</param>
        /// <param name="ffmpegArguments">The command line arguments for FFmpeg.</param>
        public FFmpegTranscodingStreamConfiguration(string ffmpegPath, string ffmpegArguments)
        {
            FFmpegPath = ffmpegPath;
            FFmpegArguments = ffmpegArguments;
        }
    }
}
