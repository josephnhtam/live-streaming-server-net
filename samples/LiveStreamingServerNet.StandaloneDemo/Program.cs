using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Insatller;
using System.Net;

namespace LiveStreamingServerNet.StandaloneDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var liveStreamingServer = CreateLiveStreamingServer();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            var app = builder.Build();

            app.UseWebSockets();
            app.UseWebSocketFlv(liveStreamingServer);

            app.UseHttpFlv(liveStreamingServer);

            app.MapStandaloneServerApiEndPoints(liveStreamingServer);
            app.UseAdminPanelUI(new AdminPanelUIOptions { BasePath = "/ui", HasHttpFlvPreview = true });

            await app.RunAsync();
            await liveStreamingServer.DisposeAsync();
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddBandwidthLimiter(10_000_000)
                    .AddStandaloneServices()
                    .AddFlv())
                .ConfigureLogging(options => options.AddConsole())
                .Build();
        }
    }
}
