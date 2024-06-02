# Custom FFmpeg Process

This guide will provide an example to use [FFmpeg](https://ffmpeg.org/) to store the RTMP stream as an MP4 archive.

### Add the required packages

```
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.StreamProcessor
```

### Configure Your Live Streaming Server

```cs
using LiveStreamingServerNet;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddStreamProcessor()
        .AddFFmpeg(options =>
        {
            options.Name = "mp4-archive";
            options.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
            options.FFmpegArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -f mp4 {outputPath}";
            options.OutputPathResolver = (contextIdentifier, streamPath, streamArguments) =>
            {
                return Task.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "mp4-archive", streamPath.Trim('/'), "output.mp4"));
            };
        })
    )
    .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .Build();

await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
```

Whenever a stream is published, a FFmpeg process with the argument `-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -f mp4 {outputPath}` will be automatically created. This process converts the RTMP stream to MP4.

Note that `{inputPath}` and `{outputPath}` are placeholders which will be replaced by the actual paths internally.
