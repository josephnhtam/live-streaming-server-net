using LiveStreamingServerNet.Transmuxer.Utilities;

namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class FFmpegTransmuxerFactoryConfiguration
    {
        public string Name { get; set; }
        public string FFmpegPath { get; set; }
        public string FFmpegArguments { get; set; }
        public int GracefulShutdownTimeoutSeconds { get; set; }
        public FFmpegOutputPathResolverDelegate OutputPathResolver { get; set; }

        [Obsolete($"Use {nameof(FFmpegArguments)} instead")]
        public string FFmpegTransmuxerArguments { get => FFmpegArguments; set => FFmpegArguments = value; }

        public FFmpegTransmuxerFactoryConfiguration()
        {
            Name = "ffmpeg";
            FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
            FFmpegArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -tune zerolatency -x264-params keyint=15:min-keyint=15 -hls_time 1 -hls_flags delete_segments -hls_list_size 20 -f hls {outputPath}";
            GracefulShutdownTimeoutSeconds = 5;
            OutputPathResolver = DefaultOutputPathResolver;
        }

        public static Task<string> DefaultOutputPathResolver(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString(), "output.m3u8"));
        }
    }

    public delegate Task<string> FFmpegOutputPathResolverDelegate(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
}
