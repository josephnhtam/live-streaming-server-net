using LiveStreamingServerNet.AdminPanelUI;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Standalone;
using LiveStreamingServerNet.Standalone.Installer;
using System.Net;

namespace LiveStreamingServerNet.StandaloneDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer();

            var app = builder.Build();

            app.UseHttpFlv();

            app.MapStandaloneServerApiEndPoints();
            app.UseAdminPanelUI(new AdminPanelUIOptions { BasePath = "/ui", HasHttpFlvPreview = true });

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(this IServiceCollection services)
        {
            return services.AddLiveStreamingServer(
                [new IPEndPoint(IPAddress.Any, 1935)],
                options => options
                    .AddBandwidthLimiter(100_000_000)
                    .AddStandaloneServices()
                    .AddFlv()
            );
        }
    }
}
