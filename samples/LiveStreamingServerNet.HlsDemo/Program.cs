using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net;

namespace LiveStreamingServerNet.HlsDemo
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
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor(options =>
                    {
                        options.AddStreamProcessorEventHandler(svc =>
                                new StreamProcessorEventListener(outputDir, svc.GetRequiredService<ILogger<StreamProcessorEventListener>>()));
                    })
                    .AddHlsTransmuxer(options => options.OutputPathResolver = new HlsOutputPathResolver(outputDir))
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }

        private class HlsOutputPathResolver : IHlsOutputPathResolver
        {
            private readonly string _outputDir;

            public HlsOutputPathResolver(string outputDir)
            {
                _outputDir = outputDir;
            }

            public ValueTask<HlsOutputPath> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                var basePath = Path.Combine(_outputDir, streamPath.Trim('/'));

                return ValueTask.FromResult(new HlsOutputPath
                {
                    ManifestOutputPath = Path.Combine(basePath, "output.m3u8"),
                    TsSegmentOutputPath = Path.Combine(basePath, "output{seqNum}.ts")
                });
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
