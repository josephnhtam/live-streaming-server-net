using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Internal.Utilities;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Reflection;

namespace LiveStreamingServerNet.HlsDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var trasmuxerOutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "TransmuxerOutput");
            new DirectoryInfo(trasmuxerOutputPath).Create();

            var builder = WebApplication.CreateBuilder(args);

            var liveStreamingServer = CreateLiveStreamingServer(trasmuxerOutputPath);

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

            app.Run();
        }

        private static (PhysicalFileProvider, FileExtensionContentTypeProvider) CreateProviders(string trasmuxerOutputPath)
        {
            var fileProvider = new PhysicalFileProvider(trasmuxerOutputPath);

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".m3u8"] = "application/x-mpegURL";

            return (fileProvider, contentTypeProvider);
        }

        private static IServer CreateLiveStreamingServer(string trasmuxerOutputPath)
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options =>
                    options
                        .AddTransmuxer(options =>
                            options.AddTransmuxerEventHandler(svc =>
                                new TransmuxerEventListener(trasmuxerOutputPath, svc.GetRequiredService<ILogger<TransmuxerEventListener>>()))
                        )
                        .AddFFmpeg(options =>
                        {
                            options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                            options.OutputPathResolver = (streamPath, streamArguments)
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

            public Task OnTransmuxerStartedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_trasmuxerOutputPath, outputPath);
                _logger.LogInformation($"Transmuxer ({identifier}) started: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }

            public Task OnTransmuxerStoppedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_trasmuxerOutputPath, outputPath);
                _logger.LogInformation($"Transmuxer ({identifier}) stopped: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }
        }
    }
}
