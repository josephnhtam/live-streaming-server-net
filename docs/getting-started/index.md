---
hide:
  - navigation
---

# Getting Started

LiveStreamingServerNet is a RTMP server framework that allows you to create your own live streaming server using .NET 7 or newer versions. It supports multiple platforms including Windows, Linux, and MacOS, and can be easily integrated into your projects as NuGet packages.

## Setting Up Your Live Streaming Server

This guide will walk you through the process of setting up your own live streaming server using LiveStreamingServerNet. By following these steps, you'll be able to create a server that can accept RTMP streams and broadcast them to viewers.

### Step 1: Initialize a New Project

Create a new .NET console application using the following command:

```
dotnet new console
```

In this guide, we will use a basic console application as the foundation for the live streaming server. However, it is not mandatory, and you are free to run a live streaming server with an ASP.NET Core application as well.

### Step 2: Add Required Packages

Add the necessary packages to the created project with these commands:

```
dotnet add package LiveStreamingServerNet
dotnet add package Microsoft.Extensions.Logging.Console
```

These commands will install the LiveStreamingServerNet package, which is the core library for building a live streaming server, as well as the Microsoft.Extensions.Logging.Console package, which allows you to log events to the console.

### Step 3: Configure Your Live Streaming Server

Edit `Program.cs` file to set up LiveStreamingServerNet:

```cs linenums="1"
using LiveStreamingServerNet;
using Microsoft.Extensions.Logging;
using System.Net;

using var server = LiveStreamingServerBuilder.Create()
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await server.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
```

This code sets up the server using LiveStreamingServerNet and configures it to log events to the console. It also specifies that the server should listen on port 1935 for RTMP streams.

### Step 4: Launch Your Live Streaming Server

Execute your live streaming server by running:

```
dotnet run
```

Now, your live streaming server should be running and ready to accept RTMP stream via 1935 port.

---

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

Assuming you have a media file at `input_file`, which could be in formats like mp4, wmv, etc. To publish it as a live stream using FFmpeg, execute the following command:

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
