using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

namespace LiveStreamingServerNet.AdaptiveHlsDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer();

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader()
                          .AllowAnyOrigin()
                          .AllowAnyMethod()
                )
            );

            var app = builder.Build();

            app.UseCors();

            // Given that the scheme is https, the port is 7138, and the stream path is live/demo,
            // the HLS stream will be available at https://localhost:7138/live/demo/output.m3u8
            app.UseHlsFiles();

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(this IServiceCollection services)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddStreamProcessor(options =>
                    {
                        options.AddStreamProcessorEventHandler(svc =>
                            new StreamProcessorEventListener(
                                svc.GetRequiredService<ILogger<StreamProcessorEventListener>>()));
                    })
                    .AddAdaptiveHlsTranscoder(options =>
                    {
                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                        options.FFprobePath = ExecutableFinder.FindExecutableFromPATH("ffprobe")!;

                        // Hardware acceleration 
                        // options.VideoDecodingArguments = "-hwaccel auto -c:v h264_cuvid";
                        // options.VideoEncodingArguments = "-c:v h264_nvenc -g 30";
                    })
            );
        }

        private class StreamProcessorEventListener : IStreamProcessorEventHandler
        {
            private readonly ILogger _logger;

            public StreamProcessorEventListener(ILogger<StreamProcessorEventListener> logger)
            {
                _logger = logger;
            }

            public Task OnStreamProcessorStartedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                _logger.LogInformation($"[{identifier}] Streaming processor {processor} started: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }

            public Task OnStreamProcessorStoppedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                _logger.LogInformation($"[{identifier}] Streaming processor {processor} stopped: {inputPath} -> {outputPath}");
                return Task.CompletedTask;
            }
        }
    }
}
