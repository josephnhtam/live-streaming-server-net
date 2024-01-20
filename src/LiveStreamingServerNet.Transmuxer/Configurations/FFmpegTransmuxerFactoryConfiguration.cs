using LiveStreamingServerNet.Transmuxer.Internal.Utilities;

namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class FFmpegTransmuxerFactoryConfiguration
    {
        public string TransmuxerIdentifier { get; set; }
        public string FFmpegPath { get; set; }
        public string FFmpegTransmuxerArguments { get; set; }
        public string OutputFileName { get; set; }

        public bool CreateWindow { get; set; } = true;
        public int GracefulShutdownTimeoutSeconds { get; set; } = 5;

        public FFmpegTransmuxerFactoryConfiguration()
        {
            TransmuxerIdentifier = "ffmpeg";
            FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
            FFmpegTransmuxerArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -tune zerolatency -x264-params keyint=15:min-keyint=15 -hls_time 1 -hls_flags delete_segments -hls_list_size 20 -f hls {outputPath}";
            OutputFileName = "output.m3u8";
        }
    }
}
