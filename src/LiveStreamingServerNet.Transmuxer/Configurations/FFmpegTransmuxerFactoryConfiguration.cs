﻿using LiveStreamingServerNet.Transmuxer.Internal.Utilities;
using System.Reflection;

namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class FFmpegTransmuxerFactoryConfiguration
    {
        public string TransmuxerIdentifier { get; set; }
        public string FFmpegPath { get; set; }
        public string FFmpegTransmuxerArguments { get; set; }
        public int GracefulShutdownTimeoutSeconds { get; set; }
        public FFmpegOutputPathResolverDelegate OutputPathResolver { get; set; }

        public FFmpegTransmuxerFactoryConfiguration()
        {
            TransmuxerIdentifier = "ffmpeg";
            FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg") ?? string.Empty;
            FFmpegTransmuxerArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -tune zerolatency -x264-params keyint=15:min-keyint=15 -hls_time 1 -hls_flags delete_segments -hls_list_size 20 -f hls {outputPath}";
            GracefulShutdownTimeoutSeconds = 5;
            OutputPathResolver = DefaultOutputPathResolver;
        }

        public static Task<string> DefaultOutputPathResolver(string streamPath, IDictionary<string, string> streamArguments)
        {
            var entryDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            return Task.FromResult(Path.Combine(entryDirectory, "Output", streamPath.Trim('/'), "output.m3u8"));
        }
    }

    public delegate Task<string> FFmpegOutputPathResolverDelegate(string streamPath, IDictionary<string, string> streamArguments);
}
