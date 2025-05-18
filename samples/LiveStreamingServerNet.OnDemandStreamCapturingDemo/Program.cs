using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.OnDemandStreamCapturingDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            var builder = Host.CreateApplicationBuilder();

            var endPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);
            builder.Services.AddLiveStreamingServer(endPoint, options =>
            {
                options.AddStreamProcessor()
                    .AddOnDemandStreamCapturer(options =>
                    {
                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                    });
            });

            builder.Services.AddHostedService<StreamCapturer>();

            var app = builder.Build();
            await app.RunAsync();
        }
    }

    public class StreamCapturer : BackgroundService
    {
        private readonly IRtmpStreamInfoManager _streamInfoManager;
        private readonly IOnDemandStreamCapturer _capturer;
        private readonly ILogger<StreamCapturer> _logger;
        private readonly string _outputDir;

        private const int MaxConcurrency = 10;

        public StreamCapturer(IRtmpStreamInfoManager streamInfoManager, IOnDemandStreamCapturer capturer, ILogger<StreamCapturer> logger)
        {
            _streamInfoManager = streamInfoManager;
            _capturer = capturer;
            _logger = logger;

            _outputDir = Path.Combine(Directory.GetCurrentDirectory(), "capture-output");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var streamInfos = _streamInfoManager.GetStreamInfos();

                foreach (var streams in streamInfos.Chunk(MaxConcurrency))
                {
                    await Task.WhenAll(streams.Select(async s =>
                    {
                        try
                        {
                            var outputDir = Path.Combine(_outputDir, s.Publisher.Id.ToString());
                            new DirectoryInfo(outputDir).Create();

                            // Use extensions such as .png, .jpg, .bmp, .tiff, .webp
                            var snapshotOutputPath = Path.Combine(outputDir, "snapshot.png");
                            await _capturer.CaptureSnapshotAsync(
                                streamPath: s.StreamPath,
                                streamArguments: s.StreamArguments,
                                outputPath: Path.Combine(_outputDir, snapshotOutputPath),
                                height: 512,
                                cancellationToken: stoppingToken
                            );

                            _logger.LogInformation("Captured stream {StreamPath} to {OutputPath}", s.StreamPath, snapshotOutputPath);

                            // Use extensions such as .webp, .webm, .gif, .mp4
                            var webpOutputPath = Path.Combine(outputDir, "clip.webp");
                            await _capturer.CaptureClipAsync(
                                streamPath: s.StreamPath,
                                streamArguments: s.StreamArguments,
                                outputPath: Path.Combine(_outputDir, webpOutputPath),
                                options: new ClipCaptureOptions(TimeSpan.FromSeconds(3))
                                {
                                    Framerate = 24,
                                    Height = 512
                                },
                                cancellationToken: stoppingToken
                            );

                            _logger.LogInformation("Captured clip from stream {StreamPath} to {OutputPath}", s.StreamPath, webpOutputPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error capturing stream {StreamPath}: {Exception}", s.StreamPath, ex);
                        }
                    }));
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
