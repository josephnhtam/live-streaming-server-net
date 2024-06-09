using LiveStreamingServerNet.Networking;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LiveStreamingServerNet.RtmpsDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            List<ServerEndPoint> endPoints =
                [new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false),
                    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 443), true)];

            await liveStreamingServer.RunAsync(endPoints);
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureServer(options => options.ConfigureSecurity(options =>
                {
                    var pfxPath = Environment.GetEnvironmentVariable("CERT_PFX_PATH")!;
                    var pfxPassword = Environment.GetEnvironmentVariable("CERT_PFX_PASSWORD")!;

                    options.ServerCertificate = new X509Certificate2(pfxPath, pfxPassword);
                }))
                .ConfigureRtmpServer(options =>
                    options.Configure(options => options.EnableGopCaching = true)
                )
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .Build();
        }
    }
}
