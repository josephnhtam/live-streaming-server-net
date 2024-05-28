# Securing Streams with RTMPS

Just as HTTP has its secure counterpart in HTTPS, RTMP also has a secure variant known as RTMPS. RTMPS enhances RTMP by incorporating an extra layer of security via TLS or SSL encryption, which is important to mitigate concerns related to piracy and cybersecurity threats.

### Step 1: Initialize a New Project and Add Required Packages

```
dotnet new console
dotnet add package LiveStreamingServerNet
dotnet add package Microsoft.Extensions.Logging.Console
```

As usual, it’s not mandatory to use a Console Application as the foundation.

### Step 2: Configure Your Live Streaming Server

LiveStreamingServerNet uses `System.Net.Security.SslStream` to internally encrypt the RTMP stream. Therefore, you need to provide a `X509Certificate2`.

Assuming you have a PFX archive file, edit the `Program.cs` file:

```cs
using LiveStreamingServerNet;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using LiveStreamingServerNet.Networking;

var pfxPath = Environment.GetEnvironmentVariable("CERT_PFX_PATH")!;
var pfxPassword = Environment.GetEnvironmentVariable("CERT_PFX_PASSWORD")!;

var serverCertificate = new X509Certificate2(pfxPath, pfxPassword);

var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureServer(options => options.ConfigureSecurity(options =>
        options.ServerCertificate = serverCertificate
    ))
    .ConfigureRtmpServer(options =>
        options.Configure(options => options.EnableGopCaching = true)
    )
    .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .Build();

await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 443), IsSecure: true));
```

LiveStreamingServerNet also supports running the live streaming server on multiple ports. Therefore, it’s possible to run the server with both the RTMP and RTMPS protocols simultaneously.

```cs
var endPoints = new List<ServerEndPoint> {
    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), IsSecure: false),
    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 443), IsSecure: true)
};

await liveStreamingServer.RunAsync(endPoints);
```

### Step 3: Launch Your Live Streaming Server

Execute your live streaming server by running:

```
dotnet run
```

Upon successful execution, your live streaming server will start running. It will be ready to accept RTMPS streams via port 443. For instance, you can test it by sending a stream to `rtmps://localhost/live/demo`.
