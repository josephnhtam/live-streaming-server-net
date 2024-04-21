# Getting Started

LiveStreamingServerNet is a RTMP server framework that allows you to create your own live streaming server using .NET 7 or newer versions. It supports multiple platforms including Windows, Linux, and MacOS, and can be easily integrated into your projects as NuGet packages.

## Setting Up Your Live Streaming Server

This guide will walk you through the process of setting up your live streaming server.

### Step 1: Initialize a New Project

Create a new .NET console application using the following command:

```
dotnet new console
```

### Step 2: Add Required Packages

Add the necessary packages to the created project with these commands:

```
dotnet add package LiveStreamingServerNet
dotnet add package Microsoft.Extensions.Logging.Console
```

### Step 3: Configure Your Live Streaming Server

Edit `Program.cs` file to set up LiveStreamingServerNet:

```
using LiveStreamingServerNet;
using Microsoft.Extensions.Logging;
using System.Net;

using var server = LiveStreamingServerBuilder.Create()
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await server.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
```

### Step 4: Launch Your Live Streaming Server

Execute your live streaming server by running:

```
dotnet run
```

Now, you live streaming server should be running and ready to accept RTMP stream via 1935 port.

## Interacting with your Live Streaming Server

With your live streaming server now up and running, you’re all set to publish and play live streams.

### Publish a Live Stream

You have the flexibility to publish your stream using a tool of your choice, such as OBS Studio or FFmpeg.

**With OBS Studio**

1. Open OBS Studio and go to "Settings".
2. In the "Settings" window, select the "Stream" tab.
3. Choose "Custom" as the "Service".
4. Enter "Server": `rtmp://localhost:1935/live` and "Stream Key": `demo`.
5. Click "OK" to save the settings.
6. Click the "Start Streaming" button in OBS Studio to begin sending live stream to the RTMP server.

**With FFmpeg**

Assume you have a media file at `input_file`, which could be in formats like mp4, wmv, etc. To publish it as a live stream using FFmpeg, execute the following command:

```
ffmpeg -re -i <input_file> -c:v libx264 -c:a aac -f flv rtmp://localhost:1935/live/demo
```

### Play the Live Stream

Now, you can play the published live stream with your favorite tool.

**With VLC Media Player**

1. Open VLC Media Player.
2. Go to the "Media" menu and select "Open Network Stream".
3. In the "Network" tab, enter the URL: `rtmp://localhost:1935/live/demo`.
4. Click the "Play" button to start playing the live stream.

**With FFplay**

Use the following command to play the live stream using FFplay

```
ffplay rtmp://localhost:1935/live/demo
```
