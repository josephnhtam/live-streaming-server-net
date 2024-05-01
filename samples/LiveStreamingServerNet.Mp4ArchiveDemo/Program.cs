using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.Mp4ArchiveDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
                e.Cancel = true;
            };

            await liveStreamingServer.RunAsync(
                new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false), cts.Token);
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddTransmuxer()
                    .AddFFmpeg(options =>
                    {
                        options.Name = "mp4-archive";
                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                        options.FFmpegArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -f mp4 {outputPath}";
                        options.OutputPathResolver = (contextIdentifier, streamPath, streamArguments) =>
                        {
                            return Task.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "mp4-archive", streamPath.Trim('/'), "output.mp4"));
                        };
                    })
                )
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .Build();
        }
    }
}
