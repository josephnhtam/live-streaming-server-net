# Serving HLS Stream via Cloud Storage Service

The following will use Azure Blob Storage as an example.

### Step 1: Initialize a New Project and Add the Required Packages

Create an empty console application and add the necessary packages using the following commands:

```
dotnet new console
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.StreamProcessor
dotnet add package LiveStreamingServerNet.StreamProcessor.AzureBlobStorage
dotnet add package Microsoft.Extensions.Logging.Console
```

### Step 2: Configure Your Live Streaming Server

Modify `Program.cs` file:

```cs linenums="1"
using Azure.Storage.Blobs;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.StreamProcessor;
using LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using System.Net;

var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
new DirectoryInfo(outputDir).Create();

var blobContainerClient = new BlobContainerClient(
    Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING"),
    Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER"));

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddStreamProcessor(options =>
        {
            options.AddHlsUploader(uploaderOptions =>
            {
                uploaderOptions.AddHlsStorageEventHandler<HlsStorageEventListener>();
                uploaderOptions.AddAzureBlobStorage(blobContainerClient);
            });
        })
        .AddHlsTransmuxer()
    )
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await liveStreamingServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935));

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
            _logger.LogInformation($"[{context.Identifier}] Main manifest {mainManifestName} stored at {mainManifest.Uri}");

        return Task.CompletedTask;
    }

    public Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, StreamProcessingContext context)
    {
        return Task.CompletedTask;
    }
}
```

This setup allows the server to receive RTMP streams, transmux them to HLS formats, and then upload the HLS files to Azure Blob Storage. Besides, the HlsStorageEventListener will log information about the stored files for monitoring purposes.

### Step 3: Launch Your Live Streaming Server

To execute your live streaming server, run the following command:

```
dotnet run
```

Once a live stream is published to the live streaming server, the corresponding stream will be automatically uploaded to the Azure Blob Storage container.
