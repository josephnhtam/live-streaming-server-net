using Azure.Storage.Blobs;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.CognitiveServices.Speech;
using System.Net;

namespace LiveStreamingServerNet.SubtitledHlsDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var azureSpeechConfig = AzureSpeechConfig.FromEnvironment();
            var azureBlobStorageConfig = AzureBlobStorageConfig.FromEnvironment();

            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "hls-output");
            new DirectoryInfo(outputDir).Create();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer(azureSpeechConfig, azureBlobStorageConfig, outputDir);

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

        private static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, AzureSpeechConfig? azureSpeechConfig, AzureBlobStorageConfig? azureBlobStorageConfig, string outputDir)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC).Include(VideoCodec.HEVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor(options =>
                    {
                        if (azureBlobStorageConfig != null)
                        {
                            var blobContainerClient = new BlobContainerClient(
                                azureBlobStorageConfig.ConnectionString, azureBlobStorageConfig.ContainerName);

                            options.AddHlsUploader(uploaderOptions =>
                            {
                                uploaderOptions.AddHlsStorageEventHandler<HlsStorageEventListener>()
                                               .AddAzureBlobStorage(blobContainerClient);
                            });
                        }
                    })
                    .AddHlsTransmuxer(options =>
                    {
                        if (azureSpeechConfig != null)
                        {
                            var subtitleTrackOptions = new SubtitleTrackOptions("Subtitle");
                            var speechConfig = SpeechConfig.FromSubscription(azureSpeechConfig.Key, azureSpeechConfig.Region);
                            var autoDetectLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new[] { "en-US", "ja-JP" });

                            options.AddAzureSpeechTranscription(subtitleTrackOptions, speechConfig, configure =>
                                configure.WithFFmpegPath(ExecutableFinder.FindExecutableFromPATH("ffmpeg")!)
                                         .WithAutoDetectLanguageConfig(autoDetectLanguageConfig)
                            );
                        }
                    })
            );
        }

        public class HlsStorageEventListener : IHlsStorageEventHandler
        {
            private readonly ILogger _logger;

            public HlsStorageEventListener(ILogger<HlsStorageEventListener> logger)
            {
                _logger = logger;
            }

            public Task OnHlsFilesStoredAsync(
                IEventContext eventContext,
                StreamProcessingContext context,
                bool initial,
                IReadOnlyList<StoredManifest> storedManifests,
                IReadOnlyList<StoredSegment> storedSegments)
            {
                if (!initial)
                    return Task.CompletedTask;

                var mainManifestName = Path.GetFileName(context.OutputPath);
                var mainManifest = storedManifests.FirstOrDefault(x => x.Name.Equals(mainManifestName));

                if (mainManifest != default)
                    _logger.LogInformation($"Main manifest {mainManifestName} stored at {mainManifest.Uri}");

                return Task.CompletedTask;
            }

            public Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, StreamProcessingContext context)
            {
                return Task.CompletedTask;
            }
        }

        private record AzureSpeechConfig(string Key, string Region)
        {
            public static AzureSpeechConfig? FromEnvironment()
            {
                var key = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
                var region = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(region))
                {
                    return null;
                }

                return new AzureSpeechConfig(key, region);
            }
        }

        private record AzureBlobStorageConfig(string ConnectionString, string ContainerName)
        {
            public static AzureBlobStorageConfig? FromEnvironment()
            {
                var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING");
                var containerName = Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER");

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                {
                    return null;
                }

                return new AzureBlobStorageConfig(connectionString, containerName);
            }
        }
    }
}
