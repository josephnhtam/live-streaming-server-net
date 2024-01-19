using LiveStreamingServerNet.Transmuxer.Internal.Utilities;

namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class FFmpegTransmuxerFactoryConfiguration
    {
        public string FFmpegPath { get; set; }
        public string FFmpegTransmuxerArguments { get; set; }
        public string OutputFileName { get; set; }

        public bool CreateWindow { get; set; } = true;
        public int GracefulShutdownTimeoutSeconds { get; set; } = 5;

        public FFmpegTransmuxerFactoryConfiguration()
        {
            FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
            FFmpegTransmuxerArguments = "-i {inputPath} -c:v libx264 -preset ultrafast -tune zerolatency -c:a aac -hls_time 1 -hls_list_size 5 -seg_duration 3 -hls_playlist_type event {outputPath}";
            OutputFileName = "output.m3u8";
        }
    }
}
