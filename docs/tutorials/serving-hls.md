# Serving HLS Live Streams

LiveStreamingServerNet provides a seamless way to the transmux RTMP streams into HLS streams using the built-in HLS transmuxer, without the need for external tools like [FFmpeg](https://ffmpeg.org/).

## Quick Setup Guide for Your Live Streaming Server

This section will guide you through adding a HLS transmuxer to convert RTMP streams into HLS streams and serving the HLS live streams with ASP.NET Core.

### Step 1: Initialize a New Project and Add Required Packages

Create an empty ASP.NET Core Web application and add the necessary packages using the following commands:

```
dotnet new web
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.StreamProcessor
dotnet add package LiveStreamingServerNet.StreamProcessor.AspNetCore
```

### Step 2: Configure Your Live Streaming Server

Edit `Program.cs` file:

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Configurations;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Net;

var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "hls-output");
new DirectoryInfo(outputDir).Create();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiveStreamingServer(
    new IPEndPoint(IPAddress.Any, 1935),
    options => options
        .AddStreamProcessor(options => options.AddStreamProcessorEventHandler<HlsTransmuxerEventListener>())
        .AddHlsTransmuxer(options => options.OutputPathResolver = new HlsTransmuxerOutputPathResolver(outputDir))
);

var app = builder.Build();

app.UseHlsFiles(new HlsServingOptions
{
    Root = outputDir,
    RequestPath = "/hls"
});

app.Run();

public class HlsTransmuxerOutputPathResolver : IHlsOutputPathResolver
{
    private readonly string _outputDir;

    public HlsTransmuxerOutputPathResolver(string outputDir)
    {
        _outputDir = outputDir;
    }

    public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        return ValueTask.FromResult(Path.Combine(_outputDir, contextIdentifier.ToString(), "output.m3u8"));
    }
}

public class HlsTransmuxerEventListener : IStreamProcessorEventHandler
{
    private readonly ILogger _logger;

    public HlsTransmuxerEventListener(ILogger<HlsTransmuxerEventListener> logger)
    {
        _logger = logger;
    }

    public Task OnStreamProcessorStartedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        _logger.LogInformation($"[{identifier}] Transmuxer {transmuxer} started: {inputPath} -> {outputPath}");
        return Task.CompletedTask;
    }

    public Task OnStreamProcessorStoppedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        _logger.LogInformation($"[{identifier}] Transmuxer {transmuxer} stopped: {inputPath} -> {outputPath}");
        return Task.CompletedTask;
    }
}
```

This code sets up the server using LiveStreamingServerNet to listen on port 1935 for RTMP streams. Whenever an RTMP stream is published to the server, a HLS transmuxer will be created to convert the RTMP stream into an HLS stream, and its manifest will be stored as `/hls-output/{streamPath}/output.m3u8`. Additionally, the `HlsTransmuxerEventListener` is added to log out the transmuxer events. And finally, the `HlsFilesMiddleware` is added to serve the generated HLS live streams at `/hls/**`.

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run --urls="https://+:8080"
```

Now, when you publish a live stream to `rtmp://localhost:1935/live/demo`, the HLS transmuxer will automatically convert it into HLS format. You can access the HLS stream by visiting `https://localhost:8080/hls/live/demo/output.m3u8`. If you need to play the HLS stream in a browser, you generally need a JavaScript library such as [hls.js](https://github.com/video-dev/hls.js) or [video.js](https://github.com/videojs/video.js).

## Filtering Video and Audio Codecs

The HLS transmuxer doesn't transcode the RTMP streams but only changes the container format from RTMP to HLS, which uses the MPEG-TS format for its media segments. This process doesn't alter the actual video or audio data and is significantly less resource-intensive compared to transcoding.

Therefore, to ensure that transmuxing works as expected, it’s necessary to verify that the incoming RTMP stream contains elementary streams with the **H.264** and **AAC** codecs.

You can add the video and codec filters like this:

```cs linenums="1"
var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
        .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
        .AddStreamProcessor()
        .AddHlsTransmuxer()
    )
    .Build();
```

## Enabling HLS Transmuxer Conditionally

Sometimes, not all the RTMP streams require transmuxing into HLS streams. In such cases, you can provide an `IStreamProcessorCondition` to the `HlsTransmuxerConfiguration` in order to selectively enable the HLS Transmuxer.

For Example, you can implement an `IStreamProcessorCondition` like this:

```cs linenums="1"
public class HlsTransmuxingCondition : IStreamProcessorCondition
{
    public ValueTask<bool> IsEnabled(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        return ValueTask.FromResult(streamArguments.GetValueOrDefault("hls", "false") == "true");
    }
}
```

And add it to the `HlsTransmuxerConfiguration`:

```cs linenums="1"
var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddStreamProcessor()
        .AddHlsTransmuxer(hlsTransmuxerConfig =>
            hlsTransmuxerConfig.Condition = new HlsTransmuxingCondition())
    )
    .Build();
```

Now, only RTMP streams published with argument `hls=true` (e.g. `rtmp://localhost:1935/live/demo?hls=true`) will be transmuxed into HLS streams.
