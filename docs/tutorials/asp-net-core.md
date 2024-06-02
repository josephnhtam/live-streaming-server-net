# Running with ASP.NET Core

LiveStreamingServerNet can run as a background service within an ASP.NET Core application.

### Step 1: Initialize a New Project and Add Required Packages

Create an empty ASP.NET Core Web application and add the necessary packages using the following commands:

```
dotnet new web
dotnet add package LiveStreamingServerNet
```

### Step 2: Configure Your Live Streaming Server

Edit `Program.cs` file:

```cs
using System.Net;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Networking.Helpers;

using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options.AddFlv())
    .ConfigureLogging(options => options.AddConsole())
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

var app = builder.Build();
await app.RunAsync();
```

This code sets up the live streaming server and the ASP.NET Core web app, while the live streaming server will run alongside the web app using port 1935.

Note that `app.UseWebSockets()` must be added before `app.UseWebSocketFlv(liveStreamingServer)` to ensure a correct WebSocket pipeline.

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run
```

!!! note

    The live streaming server created with LiveStreamingServerBuilder is self-contained and has its own IoC container, i.e. IServiceProvider.
