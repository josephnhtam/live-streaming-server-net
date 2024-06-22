using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net;

namespace LiveStreamingServerNet.AdaptiveHlsDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Output");
            new DirectoryInfo(outputDir).Create();

            using var liveStreamingServer = CreateLiveStreamingServer(outputDir);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader()
                          .AllowAnyOrigin()
                          .AllowAnyMethod()
                )
            );

            var app = builder.Build();

            app.UseCors();

            var (fileProvider, contentTypeProvider) = CreateProviders(outputDir);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = contentTypeProvider
            });

            await app.RunAsync();
        }

        private static (PhysicalFileProvider, FileExtensionContentTypeProvider) CreateProviders(string outputDir)
        {
            var fileProvider = new PhysicalFileProvider(outputDir);

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".m3u8"] = "application/x-mpegURL";

            return (fileProvider, contentTypeProvider);
        }

        private static ILiveStreamingServer CreateLiveStreamingServer(string outputDir)
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddStreamProcessor(options =>
                    {
                        options.AddStreamProcessorEventHandler(svc =>
                                new StreamProcessorEventListener(outputDir, svc.GetRequiredService<ILogger<StreamProcessorEventListener>>()));
                    })
                    .AddAdaptiveHlsTranscoder(options =>
                    {
                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                        options.FFprobePath = ExecutableFinder.FindExecutableFromPATH("ffprobe")!;

                        // Hardware acceleration 
                        // options.VideoDecodingArguments = "-hwaccel auto -c:v h264_cuvid";
                        // options.VideoEncodingArguments = "-c:v h264_nvenc -g 30";

                        options.OutputPathResolver = new HlsOutputPathResolver(outputDir);
                    })
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }

        private class HlsOutputPathResolver : IFFmpegOutputPathResolver
        {
            private readonly string _outputPath;

            public HlsOutputPathResolver(string outputPath)
            {
                _outputPath = outputPath;
            }

            public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                return ValueTask.FromResult(Path.Combine(_outputPath, streamPath.Trim('/'), "output.m3u8"));
            }
        }

        private class StreamProcessorEventListener : IStreamProcessorEventHandler
        {
            private readonly string _outputDir;
            private readonly ILogger _logger;

            public StreamProcessorEventListener(string outputDir, ILogger<StreamProcessorEventListener> logger)
            {
                _outputDir = outputDir;
                _logger = logger;
            }

            public Task OnStreamProcessorStartedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_outputDir, outputPath);
                _logger.LogInformation($"[{identifier}] Streaming processor {processor} started: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }

            public Task OnStreamProcessorStoppedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_outputDir, outputPath);
                _logger.LogInformation($"[{identifier}] Streaming processor {processor} stopped: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }
        }
    }
}
