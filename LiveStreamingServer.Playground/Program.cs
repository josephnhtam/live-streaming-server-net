using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp;
using LiveStreamingServer.Rtmp.Core.Extensions;
using Serilog;
using System.Net;

namespace LiveStreamingServer.Playground
{

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();


            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                logger.Information("Stopping...");
                cts.Cancel();
                e.Cancel = true;
            };

            var rtmpServer = RtmpServerBuilder.Create()
                .ConfigureLogging(options =>
                {
                    options.AddSerilog(logger);
                })
                .Build();

            var runTask = rtmpServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935), cts.Token);

            logger.Information("RTMP server started...");

            await runTask;
        }
    }
}
