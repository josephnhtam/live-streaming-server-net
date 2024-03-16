using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiveStreamingServerNet.Networking.Helpers
{
    public static class Extensions
    {
        public static IServiceCollection AddBackgroundServer(this IServiceCollection services, IServer server, params ServerEndPoint[] serverEndPoints)
        {
            return services.AddHostedService(svc =>
                new BackgroundServerService(
                    svc.GetRequiredService<IHostApplicationLifetime>(),
                    server, serverEndPoints
                )
           );
        }
    }
}
