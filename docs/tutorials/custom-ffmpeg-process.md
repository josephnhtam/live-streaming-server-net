# Custom FFmpeg Process

This guide will provide an example to use [FFmpeg](https://ffmpeg.org/) to store the RTMP stream as an MP4 archive.

### Add the required packages

```
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.StreamProcessor
```

### Configure Your Live Streaming Server

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddStreamProcessor()
        .AddFFmpeg(configure =>
            configure.ConfigureDefault(config =>
            {
                config.Name = "mp4-archive";
                config.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                config.FFmpegArguments = "-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -f mp4 {outputPath}";
                config.OutputPathResolver = new Mp4OutputPathResolver(Path.Combine(Directory.GetCurrentDirectory(), "mp4-archive"));
            })
        )
    )
    .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .Build();

await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));

public class Mp4OutputPathResolver : IFFmpegOutputPathResolver
{
    private readonly string _outputDir;

    public Mp4OutputPathResolver(string outputDir)
    {
        _outputDir = outputDir;
    }

    public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        return ValueTask.FromResult(Path.Combine(_outputDir, streamPath.Trim('/'), "output.mp4"));
    }
}
```

Whenever a stream is published, a FFmpeg process with the argument `-i {inputPath} -c:v libx264 -c:a aac -preset ultrafast -f mp4 {outputPath}` will be automatically created. This process converts the RTMP stream to MP4.

Note that `{inputPath}` and `{outputPath}` are placeholders which will be replaced by the actual paths internally.
