using LiveStreamingServerNet.Flv.Installer;
using System.Net;

namespace LiveStreamingServerNet.FlvDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLiveStreamingServer();

            var app = builder.Build();

            app.UseWebSockets();

            app.UseWebSocketFlv();

            app.UseHttpFlv();

            await app.RunAsync();
        }

        private static IServiceCollection AddLiveStreamingServer(this IServiceCollection services)
        {
            return services.AddLiveStreamingServer(
                [new IPEndPoint(IPAddress.Any, 1935)],
                options => options.AddFlv()
            );
        }
    }
}
