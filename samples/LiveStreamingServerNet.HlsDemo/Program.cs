using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
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
            var trasmuxerOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "TransmuxerOutput");
            new DirectoryInfo(trasmuxerOutputPath).Create();

            using var liveStreamingServer = CreateLiveStreamingServer(trasmuxerOutputPath);

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

            var (fileProvider, contentTypeProvider) = CreateProviders(trasmuxerOutputPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                ContentTypeProvider = contentTypeProvider
            });

            await app.RunAsync();
        }

        private static (PhysicalFileProvider, FileExtensionContentTypeProvider) CreateProviders(string trasmuxerOutputPath)
        {
            var fileProvider = new PhysicalFileProvider(trasmuxerOutputPath);

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".m3u8"] = "application/x-mpegURL";

            return (fileProvider, contentTypeProvider);
        }

        private static ILiveStreamingServer CreateLiveStreamingServer(string trasmuxerOutputPath)
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddTransmuxer(options =>
                    {
                        options.AddTransmuxerEventHandler(svc =>
                                new TransmuxerEventListener(trasmuxerOutputPath, svc.GetRequiredService<ILogger<TransmuxerEventListener>>()));
                    })
                    .AddHlsTransmuxer(options => options.OutputPathResolver = new HlsOutputPathResolver(trasmuxerOutputPath))
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

        public class TransmuxerEventListener : ITransmuxerEventHandler
        {
            private readonly string _trasmuxerOutputPath;
            private readonly ILogger _logger;

            public TransmuxerEventListener(string trasmuxerOutputPath, ILogger<TransmuxerEventListener> logger)
            {
                _trasmuxerOutputPath = trasmuxerOutputPath;
                _logger = logger;
            }

            public Task OnTransmuxerStartedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_trasmuxerOutputPath, outputPath);
                _logger.LogInformation($"[{identifier}] Transmuxer {transmuxer} started: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }

            public Task OnTransmuxerStoppedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_trasmuxerOutputPath, outputPath);
                _logger.LogInformation($"[{identifier}] Transmuxer {transmuxer} stopped: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }
        }
    }
}
