using LiveStreamingServerNet.Networking;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LiveStreamingServerNet.BasicDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = LiveStreamingServerBuilder.Create()
                .ConfigureLogging(options => options.AddConsole())
                .Build();

            await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
        }
    }
}
