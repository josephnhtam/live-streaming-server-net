using LiveStreamingServerNet.Networking.Server.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiveStreamingServerNet.Networking.Server.Helpers
{
    /// <summary>
    /// Extension methods for server registration.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registers server as a background service in host.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="server">Server instance to run in background.</param>
        /// <param name="serverEndPoints">Endpoints for server to listen on.</param>
        /// <returns>The service collection for chaining.</returns>
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
