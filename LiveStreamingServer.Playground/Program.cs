using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp;
using LiveStreamingServer.Rtmp.Core.Utilities;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServer.Playground
{

    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Stopping...");
                cts.Cancel();
                e.Cancel = true;
            };

            var rtmpServer = new RtmpServer();
            var runTask = rtmpServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935), cts.Token);

            Console.WriteLine("RTMP server started...");

            await runTask;
        }
    }
}
