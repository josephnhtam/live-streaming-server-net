using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations
{
    public class FFmpegTranscodingStreamConfiguration
    {
        public required string FFmpegPath { get; init; }
        public required string FFmpegArguments { get; init; }

        public FFmpegTranscodingStreamConfiguration() { }

        public FFmpegTranscodingStreamConfiguration(string ffmpegPath, string ffmpegArguments)
        {
            FFmpegPath = ffmpegPath;
            FFmpegArguments = ffmpegArguments;
        }
    }
}
