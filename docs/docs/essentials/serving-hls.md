# Serving HLS Live Streams

LiveStreamingServerNet currently supports the transmuxing of the RTMP stream to an HLS stream via [FFmpeg](https://ffmpeg.org/). To use this feature, you need to have FFmpeg installed first.

## Setting Up Your Live Streaming Server

First, this section will guide you through adding a transmuxer to convert the RTMP stream into an HLS stream.

### Step 1: Initialize a New Project and Add Required Packages

Create an empty ASP.NET Core Web application and add the necessary packages using the following commands:

```
dotnet new web
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.Transmuxer
```

In this guide, we will use an ASP.NET Core Web app as the foundation to facilitate the serving of HLS streams as static files.

### Step 2: Configure Your Live Streaming Server

Edit `Program.cs` file:

```
using LiveStreamingServerNet;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
new DirectoryInfo(outputDir).Create();

var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddTransmuxer(options => options.AddTransmuxerEventHandler<TransmuxerEventListener>())
        .AddFFmpeg(options =>
        {
            options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
            options.OutputPathResolver = (contextIdentifier, streamPath, streamArguments)
                => Task.FromResult(Path.Combine(outputDir, streamPath.Trim('/'), "output.m3u8"));
        })
    )
    .ConfigureLogging(options => options.AddConsole())
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

var app = builder.Build();

app.Run();

public class TransmuxerEventListener : ITransmuxerEventHandler
{
    private readonly ILogger _logger;

    public TransmuxerEventListener(ILogger<TransmuxerEventListener> logger)
    {
        _logger = logger;
    }

    public Task OnTransmuxerStartedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        _logger.LogInformation($"[{identifier}] Transmuxer {transmuxer} started: {inputPath} -> {outputPath}");
        return Task.CompletedTask;
    }

    public Task OnTransmuxerStoppedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        _logger.LogInformation($"[{identifier}] Transmuxer {transmuxer} stopped: {inputPath} -> {outputPath}");
        return Task.CompletedTask;
    }
}
```

This code sets up the server using LiveStreamingServerNet to listen on port 1935 for RTMP streams. Whenever an RTMP stream is published to the server, a FFmpeg process will be started to convert the RTMP stream into an HLS stream, and its manifest will be stored as `/output/{streamPath}/output.m3u8`. Additionally, the `TransmuxerEventListener` is added to log out the transmuxer events.

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run --urls="https://+:8080"
```

An output HLS stream can be viewed with a VLC player.

## Serving HLS Stream via ASP.NET Core

To serve the output HLS streams with static file middleware, you can add the snippet under `var app = builder.Build()`:

```
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/hls",
    FileProvider = new PhysicalFileProvider(outputDir),
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { [".m3u8"] = "application/x-mpegURL" }
    }
});
```

Once a live stream is published to `rtmp://localhost:1935/live/demo`, you can visit the HLS stream at `https://localhost:8080/hls/live/demo/output.m3u8`

## Serving HLS Stream via Cloud Storage Service

The following will use Azure Blob Storage as an example.

### Step 1: Add The Required Package

Add the necessary package using the following commands:

```
dotnet add package LiveStreamingServerNet.Transmuxer.AzureBlobStorage
```

### Step 2: Configure Your Live Streaming Server

Modify `Program.cs` file:

```
using Azure.Storage.Blobs;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Transmuxer;
using LiveStreamingServerNet.Transmuxer.AzureBlobStorage.Installer;
using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer;
using LiveStreamingServerNet.Transmuxer.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
new DirectoryInfo(outputDir).Create();

var blobContainerClient = new BlobContainerClient(
    Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING"),
    Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER"));

var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddTransmuxer(options =>
        {
            options.AddHlsUploader(uploaderOptions =>
            {
                uploaderOptions.AddHlsStorageEventHandler<HlsStorageEventListener>();
                uploaderOptions.AddAzureBlobStorage(blobContainerClient);
            });
        })
        .AddFFmpeg(options =>
        {
            options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
            options.OutputPathResolver = (contextIdentifier, streamPath, streamArguments)
                => Task.FromResult(Path.Combine(outputDir, streamPath.Trim('/'), "output.m3u8"));
        })
    )
    .ConfigureLogging(options => options.AddConsole())
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

var app = builder.Build();

app.Run();

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
            _logger.LogInformation($"[{context.Identifier}] Main manifest {mainManifestName} stored at {mainManifest.Uri}");

        return Task.CompletedTask;
    }

    public Task OnHlsFilesStoringCompleteAsync(IEventContext eventContext, TransmuxingContext context)
    {
        return Task.CompletedTask;
    }
}
```

This setup allows the server to receive RTMP streams, transmux them to HLS using FFmpeg, and then upload the HLS files to Azure Blob Storage. Besides, the HlsStorageEventListener will log information about the stored files for monitoring purposes.

### Step 3: Launch Your Live Streaming Server

To execute your live streaming server, run the following command:

```
dotnet run
```

Once a live stream is published to the live streaming server, the corresponding stream will be automatically uploaded to the Azure Blob Storage container.
