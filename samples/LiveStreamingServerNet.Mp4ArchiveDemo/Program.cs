using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.Mp4ArchiveDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddStreamProcessor()
                    .AddFFmpeg(configure =>
                        configure.ConfigureDefault(conifg =>
                        {
                            conifg.Name = "mp4-archive";
                            conifg.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                            conifg.FFmpegArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -f mp4 {outputPath}";
                            conifg.OutputPathResolver = new Mp4OutputPathResolver(Path.Combine(Directory.GetCurrentDirectory(), "mp4-archive"));
                        })
                    )
                )
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .Build();
        }

        private class Mp4OutputPathResolver : IFFmpegOutputPathResolver
        {
            private readonly string _outputDir;

            public Mp4OutputPathResolver(string outputDir)
            {
                _outputDir = outputDir;
            }

            public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                return ValueTask.FromResult(Path.Combine(_outputDir, streamPath.Trim('/'), "output.mp4"));
            }
        }
    }
}
