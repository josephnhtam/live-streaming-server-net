using LiveStreamingServerNet.KubernetesPod.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using System.Net;

namespace LiveStreamingServerNet.KubernetesPodDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var liveStreamingServer = CreateLiveStreamingServer();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

            builder.Services.AddHealthChecks();

            var app = builder.Build();

            app.UseHealthChecks("/hc");

            await app.RunAsync();
        }

        private static ILiveStreamingServer CreateLiveStreamingServer()
        {
            return LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options => options
                    .AddKubernetesPodServices())
                .ConfigureLogging(options => options.SetMinimumLevel(LogLevel.Debug).AddConsole())
                .Build();
        }
    }
}
