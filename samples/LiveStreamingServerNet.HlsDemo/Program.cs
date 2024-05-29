using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.Utilities;
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
            var transmuxerOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "TransmuxerOutput");
            new DirectoryInfo(transmuxerOutputPath).Create();

            using var liveStreamingServer = CreateLiveStreamingServer(transmuxerOutputPath);

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

            var (fileProvider, contentTypeProvider) = CreateProviders(transmuxerOutputPath);
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

        private static ILiveStreamingServer CreateLiveStreamingServer(string transmuxerOutputPath)
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor(options =>
                    {
                        options.AddStreamProcessorEventHandler(svc =>
                                new StreamProcessorEventListener(transmuxerOutputPath, svc.GetRequiredService<ILogger<StreamProcessorEventListener>>()));
                    })
                    .AddHlsTransmuxer(options => options.OutputPathResolver = new HlsOutputPathResolver(transmuxerOutputPath))
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }

        public class HlsOutputPathResolver : IHlsOutputPathResolver
        {
            private readonly string _outputPath;

            public HlsOutputPathResolver(string outputPath)
            {
                _outputPath = outputPath;
            }

            public Task<HlsOutputPath> ResolveOutputPath(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                var basePath = Path.Combine(_outputPath, streamPath.Trim('/'));

                return Task.FromResult(new HlsOutputPath
                {
                    ManifestOutputPath = Path.Combine(basePath, "output.m3u8"),
                    TsFileOutputPath = Path.Combine(basePath, "output{seqNum}.ts")
                });
            }
        }

        public class StreamProcessorEventListener : IStreamProcessorEventHandler
        {
            private readonly string _transmuxerOutputPath;
            private readonly ILogger _logger;

            public StreamProcessorEventListener(string transmuxerOutputPath, ILogger<StreamProcessorEventListener> logger)
            {
                _transmuxerOutputPath = transmuxerOutputPath;
                _logger = logger;
            }

            public Task OnStreamProcessorStartedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_transmuxerOutputPath, outputPath);
                _logger.LogInformation($"[{identifier}] Streaming processor {processor} started: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }

            public Task OnStreamProcessorStoppedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_transmuxerOutputPath, outputPath);
                _logger.LogInformation($"[{identifier}] Streaming processor {processor} stopped: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }
        }
    }
}
