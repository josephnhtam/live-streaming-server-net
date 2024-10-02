# Live-Streaming-Server-Net

[![Nuget](https://img.shields.io/nuget/v/LiveStreamingServerNet)](https://www.nuget.org/packages/LiveStreamingServerNet/)
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![JetBrains OSS](https://img.shields.io/badge/JetBrains-OSS-yellow.svg)](https://www.jetbrains.com/community/opensource)

Live-Streaming-Server-Net is a high-performance and flexible toolset that allows you to build your own live streaming server using .NET.

Please check the [documentation](https://josephnhtam.github.io/live-streaming-server-net/) for more details.

## Features

- **RTMP/RTMPS protocol**: Supports the RTMP and RTMPS protocols for streaming audio, video, and data.
- **HTTP-FLV/WebSocket-FLV with ASP.NET CORE**: Provides support for serving FLV live streams using HTTP-FLV and WebSocket-FLV protocols within an ASP.NET Core application.
- **Transmuxing RTMP streams into HLS streams**: Allows you to transmux RTMP streams into HLS (HTTP Live Streaming) streams using the built-in HLS transmuxer.
- **Transcoding RTMP streams into Adaptive HLS streams**: Integrates with FFmpeg to transcode RTMP streams into multiple-bitrate Adaptive HLS streams.
- **Integration with FFmpeg**: Provides support for processing the incoming RTMP stream with FFmpeg, for example, to create an MP4 archive.
- **GOP caching**: Supports caching the Group of Pictures (GOP) to ensure immediate availability of live streaming content.
- **Custom authorization**: Enables you to implement custom authorization mechanisms for accessing live streams.
- **Admin panel**: Includes an admin panel that provides an user interface for managing and monitoring the live streaming server.
- **Cloud Storage Integration**: Enabling real-time uploading of HLS files to cloud storage services like Azure Blob Storage, Google Cloud Storage, and AWS S3, which ensures scalable and efficient HLS stream distribution through CDN.

## Quick Start

### Run the RTMP Server

Create a .NET 8 console application project and add the dependencies

```
dotnet new console
dotnet add package LiveStreamingServerNet
dotnet add package Microsoft.Extensions.Logging.Console
```

Program.cs

```cs
using LiveStreamingServerNet;
using Microsoft.Extensions.Logging;
using System.Net;

using var server = LiveStreamingServerBuilder.Create()
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await server.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
```

Run the application

```
dotnet run
```

### Publish a Live Stream

#### With FFmpeg

Use the following command to publish a video as the live stream using FFmpeg

```
ffmpeg -re -i <input_file> -c:v libx264 -c:a aac -f flv rtmp://localhost:1935/live/demo
```

#### With OBS Studio

1. Open OBS Studio and go to "Settings".
2. In the "Settings" window, select the "Stream" tab.
3. Choose "Custom" as the "Service".
4. Enter "Server": `rtmp://localhost:1935/live` and "Stream Key": `demo`.
5. Click "OK" to save the settings.
6. Click the "Start Streaming" button in OBS Studio to begin sending live stream to the RTMP server.

### Play the Live Stream

#### With FFplay

Use the following command to play the live stream using FFplay

```
ffplay rtmp://localhost:1935/live/demo
```

#### With VLC Media Player

1. Open VLC Media Player.
2. Go to the "Media" menu and select "Open Network Stream".
3. In the "Network" tab, enter the URL: `rtmp://localhost:1935/live/demo`.
4. Click the "Play" button to start playing the live stream.

## NuGet Packages

<table>
  <thead>
    <tr>
      <th>Package</th>
      <th>Latest Version</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <th>LiveStreamingServerNet</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Standalone</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Standalone"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Standalone.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.AdminPanelUI</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.AdminPanelUI"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.AdminPanelUI.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Flv</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Flv"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Flv.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Networking</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Networking"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Networking.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Rtmp</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor.AmazonS3</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AmazonS3"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AmazonS3.svg?logo=nuget"></a></td>
    </tr>
	<tr>
      <th>LiveStreamingServerNet.StreamProcessor.AspNetCore</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AspNetCore"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AspNetCore.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor.AzureBlobStorage</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.AzureBlobStorage"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.AzureBlobStorage.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.StreamProcessor.GoogleCloudStorage.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Utilities</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Utilities"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Utilities?logo=nuget"></a></td>
    </tr>
  </tbody>
</table>

## License

This project is licensed under the terms of the [MIT license](https://github.com/josephnhtam/live-streaming-server-net/blob/master/LICENSE).

## Acknowledgments

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="./images/jetbrains-white.svg">
  <source media="(prefers-color-scheme: light)" srcset="./images/jetbrains.svg">
  <img src="./images/jetbrains.svg" alt="JetBrains" width="200"/>
</picture>

Special thanks to [JetBrains](https://www.jetbrains.com/) for providing the open-source software license that supports the development of this project.
