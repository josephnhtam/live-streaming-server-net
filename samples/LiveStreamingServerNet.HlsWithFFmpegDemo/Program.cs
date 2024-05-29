using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.StreamProcessor.Contracts;
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
                .ConfigureServer(options => options
                    .ConfigureNetwork(options =>
                    {
                        options.NoDelay = true;
                        options.FlushingInterval = TimeSpan.FromMilliseconds(300);
                    }))
                .ConfigureRtmpServer(options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddStreamProcessor(options =>
                    {
                        options.AddStreamProcessorEventHandler(svc =>
                                new StreamProcessorEventListener(transmuxerOutputPath, svc.GetRequiredService<ILogger<StreamProcessorEventListener>>()));
                    })
                    .AddFFmpeg(options =>
                    {
                        options.FFmpegArguments =
                                    "-i {inputPath} -c:v copy -c:a copy " +
                                    "-preset ultrafast -tune zerolatency -hls_time 1 " +
                                    "-hls_flags delete_segments -hls_list_size 20 -f hls {outputPath}";

                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                        options.OutputPathResolver = (contextIdentifier, streamPath, streamArguments)
                            => Task.FromResult(Path.Combine(transmuxerOutputPath, streamPath.Trim('/'), "output.m3u8"));
                    })
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
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
