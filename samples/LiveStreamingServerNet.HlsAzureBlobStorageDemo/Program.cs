using Azure.Storage.Blobs;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.StreamProcessor;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.HlsAzureBlobStorageDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddStreamProcessor(options =>
                    {
                        var blobContainerClient = new BlobContainerClient(
                            Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING"),
                            Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER"));

                        options.AddHlsUploader(uploaderOptions =>
                        {
                            uploaderOptions.AddHlsStorageEventHandler<HlsStorageEventListener>()
                                           .AddAzureBlobStorage(blobContainerClient);
                        });
                    })
                    .AddFFmpeg(options =>
                        options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!
                    )
                )
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
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
            IReadOnlyList<StoredTsSegment> storedTsSegments)
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
}
