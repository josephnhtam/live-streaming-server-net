using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net;

namespace LiveStreamingServerNet.HlsDemoWithFFmpeg
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

        private static (PhysicalFileProvider, FileExtensionContentTypeProvider) CreateProviders(string transmuxerOutputPath)
        {
            var fileProvider = new PhysicalFileProvider(transmuxerOutputPath);

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
                    .AddFFmpeg(options =>
                    {
                        options.FFmpegArguments =
                                    "-i {inputPath} -c:v copy -c:a copy " +
                                    "-preset ultrafast -tune zerolatency -hls_time 1 " +
                                    "-hls_flags delete_segments -hls_list_size 20 -f hls {outputPath}";

                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                        options.OutputPathResolver = new HlsOutputPathResolver(outputDir);
                    })
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }

        private class HlsOutputPathResolver : IFFmpegOutputPathResolver
        {
            private readonly string _outputDir;

            public HlsOutputPathResolver(string outputDir)
            {
                _outputDir = outputDir;
            }

            public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                return ValueTask.FromResult(Path.Combine(_outputDir, streamPath.Trim('/'), "output.m3u8"));
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
