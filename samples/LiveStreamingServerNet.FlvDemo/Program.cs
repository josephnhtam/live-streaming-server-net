using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using System.Net;

namespace LiveStreamingServerNet.FlvDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            var app = builder.Build();

            app.UseWebSockets();
            app.UseWebSocketFlv(liveStreamingServer);

            app.UseHttpFlv(liveStreamingServer);

            await app.RunAsync();
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options.AddFlv())
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }
}
