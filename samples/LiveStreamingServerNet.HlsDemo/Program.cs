using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

namespace LiveStreamingServerNet.HlsDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "hls-output");
            new DirectoryInfo(outputDir).Create();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer(outputDir);

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
            // the HLS stream will be available at https://localhost:7138/hls/live/demo/output.m3u8
            app.UseHlsFiles(new HlsServingOptions
            {
                Root = outputDir,
                RequestPath = "/hls"
            });

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(this IServiceCollection services, string outputDir)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor(options =>
                    {
                        options.AddStreamProcessorEventHandler(svc =>
                                new StreamProcessorEventListener(outputDir, svc.GetRequiredService<ILogger<StreamProcessorEventListener>>()));
                    })
                    .AddHlsTransmuxer(options => options.OutputPathResolver = new HlsOutputPathResolver(outputDir))
            );
        }

        private class HlsOutputPathResolver : IHlsOutputPathResolver
        {
            private readonly string _outputDir;

            public HlsOutputPathResolver(string outputDir)
            {
                _outputDir = outputDir;
            }

            public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                return ValueTask.FromResult(Path.Combine(_outputDir, contextIdentifier.ToString(), "output.m3u8"));
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
