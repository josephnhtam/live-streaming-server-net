# HLS Subtitle Transcription

LiveStreamingServerNet enhances HLS streams with real-time subtitle transcription. By integrating with speech recognition services, it converts live audio into WebVTT subtitles delivered alongside your streams. Azure AI Speech Service integration is currently available, with support for additional providers planned.

## Install FFmpeg

To enable the HLS subtitle transcription, [FFmpeg](https://ffmpeg.org/) must be installed in advance to support transcoding the audio stream into the format that the speech recognition service accepts.

By default, the `FFmpeg` and `FFprobe` executables are located using the helper function `ExecutableFinder.FindExecutableFromPATH`, which first searches through all the directories defined in the `PATH` environment variable and then the current directory.

## Quick Setup Guide for Your Live Streaming Server with HLS Subtitle Transcription

This section will guide you through setting up an HLS transmuxer to convert RTMP streams into HLS streams, enable HLS subtitle transcription for real-time subtitle generation, and serve these live HLS streams using ASP.NET Core.

### Step 1: Initialize a New Project and Add Required Packages

Create an empty ASP.NET Core Web application and add the necessary packages using the following commands:

```
dotnet new web
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.AdminPanelUI
dotnet add package LiveStreamingServerNet.Standalone
dotnet add package LiveStreamingServerNet.StreamProcessor
dotnet add package LiveStreamingServerNet.StreamProcessor.AspNetCore
dotnet add package LiveStreamingServerNet.StreamProcessor.AzureAISpeech
```

The packages `LiveStreamingServerNet.AdminPanelUI`, `LiveStreamingServerNet.Standalone` and `LiveStreamingServerNet.StreamProcessor.AspNetCore` are optional for launching the admin panel UI and serving HLS files for previewing the real-time subtitles.

### Step 2: Configure Your Live Streaming Server

Edit `Program.cs` file:

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Installer;
using LiveStreamingServerNet.StreamProcessor.AspNetCore.Installer;
using LiveStreamingServerNet.StreamProcessor.AzureAISpeech.Installer;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.CognitiveServices.Speech;
using System.Net;

var speechKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
var speechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiveStreamingServer(
    new IPEndPoint(IPAddress.Any, 1935),
    options => options
        .AddStandaloneServices()
        .Configure(options => options.EnableGopCaching = false)
        .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC).Include(VideoCodec.HEVC))
        .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
        .AddStreamProcessor()
        .AddHlsTransmuxer(options =>
        {
            var subtitleTrackOptions = new SubtitleTrackOptions("Subtitle");
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            var autoDetectLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(new[] { "en-US", "ja-JP" });

            options.AddAzureSpeechTranscription(subtitleTrackOptions, speechConfig, configure =>
                configure.WithFFmpegPath(ExecutableFinder.FindExecutableFromPATH("ffmpeg")!)
                         .WithAutoDetectLanguageConfig(autoDetectLanguageConfig)
            );
        })
);

var app = builder.Build();

app.UseHlsFiles();
app.MapStandaloneServerApiEndPoints();
app.UseAdminPanelUI(new AdminPanelUIOptions { BasePath = "/ui", HasHlsPreview = true });

app.Run();
```

This code sets up the server using LiveStreamingServerNet to listen on port 1935 for RTMP streams. Whenever an RTMP stream is published to the server, a HLS transmuxer will be created to convert the RTMP stream into an HLS stream, also an Azure Speech subtitle transcriber will be created to transcribe the audio stream into a WebVTT subtitle stream.

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run --urls="https://+:8080"
```

Once your server is running, publish a live stream to the RTMP endpoint, for example, `rtmp://localhost:1935/live/demo`. The server will automatically convert the stream into HLS format and perform real-time subtitle transcription using Azure AI Speech Service. You can preview the HLS stream (with subtitles) via the Admin Panel UI available at `https://localhost:8080/ui`, or access the HLS stream directly by visiting `https://localhost:8080/live/demo/output.m3u8`.
