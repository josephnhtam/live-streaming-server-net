using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Utilities;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations
{
    public class FFmpegProcessConfiguration
    {
        public string Name { get; set; }
        public string FFmpegPath { get; set; }
        public string FFmpegArguments { get; set; }
        public int GracefulShutdownTimeoutSeconds { get; set; }
        public IFFmpegOutputPathResolver OutputPathResolver { get; set; }
        public IStreamProcessorCondition Condition { get; set; }

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
