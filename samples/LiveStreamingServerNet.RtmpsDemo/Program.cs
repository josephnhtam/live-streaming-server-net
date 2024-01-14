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
            var server = LiveStreamingServerBuilder.Create()
                .ConfigureServer(options => options.ConfigureSecurity(options =>
                {
                    var pfxPath = Environment.GetEnvironmentVariable("CERT_PFX_PATH")!;
                    var pfxPassword = Environment.GetEnvironmentVariable("CERT_PFX_PASSWORD")!;

                    options.ServerCertificate = new X509Certificate2(pfxPath, pfxPassword);
                }))
                .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .Build();

            IList<ServerEndPoint> endPoints =
                [new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false),
                    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 443), true)];

            using var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
                e.Cancel = true;
            };

            await server.RunAsync(endPoints, cts.Token);
        }
    }
}
