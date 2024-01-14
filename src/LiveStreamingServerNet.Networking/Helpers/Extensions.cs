using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LiveStreamingServerNet.Networking.Helpers
{
    public static class Extensions
    {
        public static IServiceCollection AddBackgroundServer(this IServiceCollection services, IServer server, IPEndPoint serverEndPoint)
        {
            return services.AddHostedService(_ => new BackgroundServerService(server, serverEndPoint));
        }
    }
}
