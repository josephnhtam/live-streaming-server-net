using Azure.Storage.Blobs;
using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Installer;
using LiveStreamingServerNet.StreamProcessor;
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

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer(azureSpeechConfig, azureBlobStorageConfig);

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader()
                          .AllowAnyOrigin()
                          .AllowAnyMethod()
                )
            );

            var app = builder.Build();

            app.UseCors();

            app.UseHlsFiles();

            app.UseAdminPanelUI(new AdminPanelUIOptions
            {
                // The Admin Panel UI will be available at https://localhost:7000/ui
                BasePath = "/ui",

                // The Admin Panel UI will access HLS streams at https://localhost:7000/{streamPath}/output.m3u8
                HasHlsPreview = true,
                HlsUriPattern = "{streamPath}/output.m3u8"
            });

            app.MapStandaloneServerApiEndPoints();

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, AzureSpeechConfig? azureSpeechConfig, AzureBlobStorageConfig? azureBlobStorageConfig)
        {
            return services.AddLiveStreamingServer(
                new IPEndPoint(IPAddress.Any, 1935),
                options => options
                    .AddStandaloneServices()
                    .Configure(options => options.EnableGopCaching = false)
                    .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC).Include(VideoCodec.HEVC))
                    .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                    .AddStreamProcessor(options =>
                    {
                        // Upload HLS files (including m3u8, ts, webvtt) to Azure Blob Storage if it is configured
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
                        // Add subtitle transcription with Azure AI Speech if it is configured
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
