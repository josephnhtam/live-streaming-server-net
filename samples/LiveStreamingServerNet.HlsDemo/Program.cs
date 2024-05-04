using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Utilities;
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
                    .AddTransmuxer(options =>
                    {
                        options.AddTransmuxerEventHandler(svc =>
                                new TransmuxerEventListener(trasmuxerOutputPath, svc.GetRequiredService<ILogger<TransmuxerEventListener>>()));
                    })
                    .AddFFmpeg(options =>
                    {
                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                        options.OutputPathResolver = (contextIdentifier, streamPath, streamArguments)
                            => Task.FromResult(Path.Combine(trasmuxerOutputPath, streamPath.Trim('/'), "output.m3u8"));
                    })
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
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
