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

```cs linenums="1"
using System.Net;
using LiveStreamingServerNet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiveStreamingServer([new IPEndPoint(IPAddress.Any, 1935)]);

var app = builder.Build();

await app.RunAsync();
```

This code adds the live streaming server to the ASP.NET Core web app, while the live streaming server will in the background using port 1935.

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run
```