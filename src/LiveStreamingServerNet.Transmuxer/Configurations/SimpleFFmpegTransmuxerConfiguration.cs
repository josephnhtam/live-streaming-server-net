using LiveStreamingServerNet.Transmuxer.Internal.Utilities;

namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class SimpleFFmpegTransmuxerConfiguration
    {
        public required string FFmpegPath { get; set; }
        public required string FFmpegTransmuxerArguments { get; set; }

        public SimpleFFmpegTransmuxerConfiguration()
        {
            FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
            FFmpegTransmuxerArguments = "-i {inputPath} -c:v libx264 -preset ultrafast -tune zerolatency -c:a aac -hls_time 1 -hls_list_size 5 -seg_duration 3 -hls_playlist_type event {outputDirPath}/output.m3u8";
        }
    }
}
