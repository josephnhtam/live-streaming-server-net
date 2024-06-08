using Azure.Storage.Blobs;
using LiveStreamingServerNet.KubernetesPod.Installer;
using LiveStreamingServerNet.KubernetesPod.Redis.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.Utilities.Contracts;
using StackExchange.Redis;
using System.Net;

namespace LiveStreamingServerNet.KubernetesPodDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            using var liveStreamingServer = await CreateLiveStreamingServerAsync(builder.Configuration);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            builder.Services.AddHealthChecks();

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyHeader()
                          .AllowAnyOrigin()
                          .AllowAnyMethod()
                )
            );

            var app = builder.Build();

            app.UseCors();

            app.UseHealthChecks("/hc");

            await app.RunAsync();
        }

        private static async Task<ILiveStreamingServer> CreateLiveStreamingServerAsync(IConfiguration configuration)
        {
            var redisConn = await CreateRedisConn(configuration);
            var blobContainerClient = CreateBlobContainerClient(configuration);

            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(rtmpServerConfigurator =>
                {
                    if (blobContainerClient != null)
                    {
                        rtmpServerConfigurator
                            .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
                            .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
                            .AddStreamProcessor(streamProcessingConfigurator =>
                            {
                                streamProcessingConfigurator.AddHlsUploader(hlsUploaderConfigurator =>
                                {
                                    hlsUploaderConfigurator.AddAzureBlobStorage(blobContainerClient);
                                    hlsUploaderConfigurator.AddHlsStorageEventHandler<HlsStorageEventHandler>();
                                });
                            })
                            .AddHlsTransmuxer();
                    }

                    rtmpServerConfigurator.AddKubernetesPodServices(podConfigurator =>
                    {
                        podConfigurator.AddStreamRegistry(registryOptions =>
                            registryOptions.UseRedisStore(redisConn)
                        );
                    });
                })
                .ConfigureLogging(options => options.SetMinimumLevel(LogLevel.Trace).AddConsole())
                .Build();
        }

        private static BlobContainerClient? CreateBlobContainerClient(IConfiguration configuration)
        {
            var azureBlobStorageConnStr = configuration.GetConnectionString("AzureBlobStorage");
            var azureBlobContainer = configuration.GetValue<string>("AzureBlobContainer");

            if (string.IsNullOrEmpty(azureBlobStorageConnStr) || string.IsNullOrEmpty(azureBlobContainer))
                return null;

            var blobServiceClient = new BlobServiceClient(azureBlobStorageConnStr);
            return blobServiceClient.GetBlobContainerClient(azureBlobContainer);
        }

        private static async Task<ConnectionMultiplexer> CreateRedisConn(IConfiguration configuration)
        {
            var redisConnStr = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("Redis connection string is required");
            return await ConnectionMultiplexer.ConnectAsync(redisConnStr);
        }

        private class HlsStorageEventHandler : IHlsStorageEventHandler
        {
            private readonly ILogger _logger;

            public HlsStorageEventHandler(ILogger<HlsStorageEventHandler> logger)
            {
                _logger = logger;
            }

            public Task OnHlsFilesStoredAsync(
                IEventContext eventContext,
                StreamProcessingContext context,
                bool initial,
                IReadOnlyList<StoredManifest> storedManifests,
                IReadOnlyList<StoredTsSegment> storedTsSegments)
            {
                if (!initial)
                    return Task.CompletedTask;

                var mainManifestName = Path.GetFileName(context.OutputPath);
                var mainManifest = storedManifests.FirstOrDefault(x => x.Name.Equals(mainManifestName));

                if (mainManifest != default)
                    _logger.LogInformation($"[{context.Identifier}] Main manifest {mainManifestName} stored at {mainManifest.Uri}");
                else
                    _logger.LogError($"[{context.Identifier}] Main manifest {mainManifestName} not found");

                return Task.CompletedTask;
            }

            public Task OnHlsFilesStoringCompleteAsync(
                IEventContext eventContext,
                StreamProcessingContext context)
            {
                _logger.LogInformation($"[{context.Identifier}] HLS files storing complete");
                return Task.CompletedTask;
            }
        }
    }
}
