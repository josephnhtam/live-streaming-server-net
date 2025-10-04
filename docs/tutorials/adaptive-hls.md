# Adaptive Bitrate HLS

To generate HLS streams at multiple bitrates, transcoding is required. Therefore, LiveStreamingServerNet integrates with [FFmpeg](https://ffmpeg.org/) to enable Adaptive Bitrate HLS Transcoding. However, it’s important to note that transcoding is more resource-intensive compared to transmuxing.

## Install FFmpeg

To use the Adaptive HLS Transcoder, [FFmpeg](https://ffmpeg.org/) must be installed in advance.

By default, the `FFmpeg` and `FFprobe` executables are located using the helper function `ExecutableFinder.FindExecutableFromPATH`, which first searches through all the directories defined in the `PATH` environment variable and then the current directory.

## Adaptive HLS Transcoder

Similar to the built-in HLS Transmuxer, the resulting HLS files can be served via ASP.NET Core and Cloud Storage Services. Please refer to the previous tutorials for the serving part. This section will focus on enabling the Adaptive HLS Transcoder.

### Step 1: Add the Required Packages

Add the necessary packages using the following commands:

```
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.StreamProcessor
```

### Step 2: Configure Your Live Streaming Server

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer;
using LiveStreamingServerNet.StreamProcessor.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;

var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "output");

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddStreamProcessor()
        .AddAdaptiveHlsTranscoder(configure =>
            configure.ConfigureDefault(config => 
            {
                config.FFmpegPath = ExecutableFinder.FindExecutableFromPATH("ffmpeg")!;
                config.FFprobePath = ExecutableFinder.FindExecutableFromPATH("ffprobe")!;
                config.OutputPathResolver = new HlsOutputPathResolver(outputDirectory);
    
                config.DownsamplingFilters = new DownsamplingFilter[]{
                    new DownsamplingFilter(
                        Name: "360p",
                        Height: 360,
                        MaxVideoBitrate: "600k",
                        MaxAudioBitrate: "64k"
                    ),
    
                    new DownsamplingFilter(
                        Name: "480p",
                        Height: 480,
                        MaxVideoBitrate: "1500k",
                        MaxAudioBitrate: "128k"
                    ),
    
                    new DownsamplingFilter(
                        Name: "720p",
                        Height: 720,
                        MaxVideoBitrate: "3000k",
                        MaxAudioBitrate: "256k"
                    )
                };
            })
        )
    )
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await liveStreamingServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935));

public class HlsOutputPathResolver : IFFmpegOutputPathResolver
{
    private readonly string _outputPath;

    public HlsOutputPathResolver(string outputPath)
    {
        _outputPath = outputPath;
    }

    public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
    {
        return ValueTask.FromResult(Path.Combine(_outputPath, streamPath.Trim('/'), "output.m3u8"));
    }
}
```

With this configuration, LiveStreamingServerNet will create an Adaptive HLS Transcoder whenever an RTMP stream is published to port 1935. The Adaptive HLS Transcoder will transcode the incoming stream into at most three streams with resolutions of 360p, 480p, and 720p, depending on the resolution of the incoming stream.

### Step 3: Launch Your Live Streaming Server

To execute your live streaming server, run the following command:

```
dotnet run
```

## Optimizing the Performance

### Bypassing Audio Transcoding

Transcoding demands significant computational resources. Therefore, bypassing audio transcoding can potentially lead to a reduction in resource utilization. However, this strategy requires that the incoming audio stream is encoded with AAC codec.

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Installer;
using Microsoft.Extensions.Logging;
using System.Net;

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddAudioCodecFilter(builder => builder.Include(AudioCodec.AAC))
        .AddStreamProcessor()
        .AddAdaptiveHlsTranscoder(config =>
            config.AudioEncodingArguments = "-c:a copy"
        )
    )
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await liveStreamingServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
```

### Hardware Acceleration

[FFmpeg](https://ffmpeg.org/) supports hardware acceleration. Please refer to [HWAccelIntro](https://trac.ffmpeg.org/wiki/HWAccelIntro) for more details.

For example

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Installer;
using Microsoft.Extensions.Logging;
using System.Net;

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options
        .AddVideoCodecFilter(builder => builder.Include(VideoCodec.AVC))
        .AddStreamProcessor()
        .AddAdaptiveHlsTranscoder(config =>
        {
            config.VideoDecodingArguments = "-hwaccel auto -c:v h264_cuvid";
            config.VideoEncodingArguments = "-c:v h264_nvenc -g 30";
        })
    )
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await liveStreamingServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
```
