using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Internal.Utilities;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
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
                            options.Configure(options =>
                            {
                                options.InputBasePath = "rtmp://localhost:1935";
                                options.OutputDirectoryPath = trasmuxerOutputPath;
                            })
                            .AddTransmuxerEventHandler<TransmuxerEventListener>()
                        )
                        .UseFFmpeg(options =>
                        {
                            options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                            options.CreateWindow = true;
                        })
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }

        public class TransmuxerEventListener : ITransmuxerEventHandler
        {
            private readonly RemuxingConfiguration _config;
            private readonly ILogger<TransmuxerEventListener> _logger;

            public TransmuxerEventListener(IOptions<RemuxingConfiguration> config, ILogger<TransmuxerEventListener> logger)
            {
                _config = config.Value;
                _logger = logger;
            }

            public Task OnTransmuxerStartedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_config.OutputDirectoryPath, outputPath);
                _logger.LogInformation($"Transmuxer started: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }

            public Task OnTransmuxerStoppedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
            {
                outputPath = Path.GetRelativePath(_config.OutputDirectoryPath, outputPath);
                _logger.LogInformation($"Transmuxer stopped: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }
        }
    }
}
