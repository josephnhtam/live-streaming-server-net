using Azure.Storage.Blobs;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Transmuxer;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer;
using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Utilities;
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

            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
                e.Cancel = true;
            };

            await liveStreamingServer.RunAsync(
                new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false), cts.Token);
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddTransmuxer(options =>
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
            TransmuxingContext context,
            bool initial,
            IReadOnlyList<StoredManifest> storedManifests,
            IReadOnlyList<StoredTsFile> storedTsFiles)
        {
            if (!initial)
                return Task.CompletedTask;

            var mainManifestName = Path.GetFileName(context.OutputPath);
            var mainManifest = storedManifests.FirstOrDefault(x => x.Name.Equals(mainManifestName));

            if (mainManifest != default)
                _logger.LogInformation($"Main manifest {mainManifestName} stored at {mainManifest.Uri}");

            return Task.CompletedTask;
        }

        public Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, TransmuxingContext context)
        {
            return Task.CompletedTask;
        }
    }
}
