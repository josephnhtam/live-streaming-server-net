using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.Transcriptions.Configurations
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

        public static FFmpegTranscodingStreamConfiguration PCM16MonoTranscoding(string? ffmpegPath = null)
        {
            return new FFmpegTranscodingStreamConfiguration
            {
                FFmpegPath = ffmpegPath ?? ExecutableFinder.FindExecutableFromPATH("ffmpeg") ??
                    throw new ArgumentException("FFmpeg executable not found"),

                FFmpegArguments = "-i pipe:0 -vn -f s16le -acodec pcm_s16le -ac 1 -ar 16000 pipe:1"
            };
        }
    }
}
