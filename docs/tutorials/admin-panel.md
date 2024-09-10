# Adding an Admin Panel

This guide will provide an explanation about adding an admin panel to the live streaming server. The admin panel allows you to browse the list of live streams, delete them, and preview the live streams via HTTP-FLV.

### Step 1: Initialize a New Project and Add Required Packages

An admin panel is served by a middleware of ASP.NET Core. Create an empty ASP.NET Core Web application and add the necessary packages using the following commands:

```
dotnet new web
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.AdminPanelUI
dotnet add package LiveStreamingServerNet.Standalone
dotnet add package LiveStreamingServerNet.Flv
```

The `LiveStreamingServerNet.AdminPanelUI` package is responsible for supplying the admin panel user interface. `The LiveStreamingServerNet.Standalone` package delivers the Web API endpoints that the admin panel UI utilizes. Meanwhile, the `LiveStreamingServerNet.Flv` package enables the HTTP-FLV preview functionality.

### Step 2: Configure Your Live Streaming Server

Edit `Program.cs` file:

```cs linenums="1"
using LiveStreamingServerNet;
using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Installer;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLiveStreamingServer(
    new IPEndPoint(IPAddress.Any, 1935),
    options => options.AddStandaloneServices().AddFlv()
);

var app = builder.Build();

app.UseHttpFlv();
app.MapStandaloneServerApiEndPoints();
app.UseAdminPanelUI(new AdminPanelUIOptions { BasePath = "/ui", HasHttpFlvPreview = true });

app.Run();
```

This code adds the live streaming server to the ASP.NET Core web app, while the live streaming server will run in the background using port 1935. In addition, the web app will serve both HTTP-FLV and the admin panel UI, as well as the API endpoints required by the admin panel.

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running the following command:

```
dotnet run --urls="https://+:8080"
```

Now, your live streaming server should be running and ready to accept RTMP streams via port 1935, and you can visit the admin panel at `https://localhost:8080/ui`.

### Preview of the Admin Panel

![Admin Panel](../../assets/images/admin-panel.jpeg)

![HTTP-FLV Preview](../../assets/images/http-flv-preview.jpeg)
