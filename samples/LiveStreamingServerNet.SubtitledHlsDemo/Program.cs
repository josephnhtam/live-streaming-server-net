using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.CognitiveServices.Speech;
using System.Net;

namespace LiveStreamingServerNet.SubtitledHlsDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var speechKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY") ??
                throw new InvalidOperationException("AZURE_SPEECH_KEY environment variable is not set.");

            var speechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION") ??
                throw new InvalidOperationException("AZURE_SPEECH_REGION environment variable is not set.");

            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "hls-output");
            new DirectoryInfo(outputDir).Create();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer(speechKey, speechRegion, outputDir);

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

        private static IServiceCollection AddLiveStreamingServer(this IServiceCollection services, string speechKey, string speechRegion, string outputDir)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC).Include(VideoCodec.HEVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor()
                    .AddHlsTransmuxer(options =>
                    {
                        options.Configure(config =>
                        {
                            config.OutputPathResolver = new HlsOutputPathResolver(outputDir);
                        });

                        var subtitleTrackOptions = new SubtitleTrackOptions("Subtitle");
                        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                        var autoDetectLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new[] { "en-US", "ja-JP" });

                        options.AddAzureSpeechTranscription(subtitleTrackOptions, speechConfig, configure =>
                            configure.WithFFmpegPath(ExecutableFinder.FindExecutableFromPATH("ffmpeg")!)
                                     .WithAutoDetectLanguageConfig(autoDetectLanguageConfig)
                        );
                    })
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
    }
}
