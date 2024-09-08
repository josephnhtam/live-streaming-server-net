# Serving FLV Live Streams

This guide will introduce the way to serve live streams via HTTP-FLV and WebSocket-FLV.

### Step 1: Initialize a New Project and Add Required Packages

Both HTTP-FLV and WebSocket-FLV will be served via middlewares of ASP.NET Core. Create an empty ASP.NET Core Web application and add the necessary packages using the following commands:

```
dotnet new web
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.Flv
```

### Step 2: Configure Your Live Streaming Server

Edit `Program.cs` file:

```cs linenums="1"
using System.Net;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Flv.Installer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiveStreamingServer(
    [new IPEndPoint(IPAddress.Any, 1935)],
    options => options.AddFlv()
);

var app = builder.Build();

app.UseWebSockets();

app.UseWebSocketFlv();

app.UseHttpFlv();

await app.RunAsync();
```

This code sets up the live streaming server and the ASP.NET Core web app, while the live streaming server will run alongside the web app using port 1935. In addition, the web app will serve both HTTP-FLV and WebSocket-FLV.

Note that `app.UseWebSockets()` must be added before `app.UseWebSocketFlv(liveStreamingServer)` to ensure a correct WebSocket pipeline.

If CORS is required, you can add the CORS service and middleware as usual. For example:

```cs linenums="1"
using System.Net;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Flv.Installer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiveStreamingServer(
    [new IPEndPoint(IPAddress.Any, 1935)],
    options => options.AddFlv()
);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyOrigin()
              .AllowAnyMethod()
    )
);

var app = builder.Build();

app.UseWebSockets();

app.UseWebSocketFlv();

app.UseHttpFlv();

await app.RunAsync();
```

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run --urls="https://+:8080"
```

Now, your live streaming server should be running and ready to accept RTMP streams via port 1935.

Once a live stream is published to `rtmp://localhost:1935/live/demo`, you can visit the HTTP-FLV live stream at

```
https://localhost:8080/live/demo.flv
```

or WebSocket-FLV live stream at

```
wss://localhost:8080/live/demo.flv
```
