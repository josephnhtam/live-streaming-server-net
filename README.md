# Live-Streaming-Server-Net

This is a .NET implementation of RTMP server.

> This project is still under active development

## Features

- RTMP/RTMPS protocol
- HTTP-FLV/WebSocket-FLV with ASP.NET CORE
- GOP caching
- Custom authorization

## Quick Start

### Run the RTMP Server

Create a .NET 8 console application

```
using LiveStreamingServerNet;
using Microsoft.Extensions.Logging;
using System.Net;

var server = LiveStreamingServerBuilder.Create()
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await server.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
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
