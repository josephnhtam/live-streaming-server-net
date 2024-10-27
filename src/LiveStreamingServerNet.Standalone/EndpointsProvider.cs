using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Standalone.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone
{
    /// <summary>
    /// Provides extension methods for mapping standalone RTMP server API endpoints.
    /// </summary>
    public static class EndpointsProvider
    {
        /// <summary>
        /// Maps all standalone RTMP server API endpoints using a provided server instance.
        /// </summary>
        /// <param name="builder">The endpoint route builder</param>
        /// <param name="server">The server instance</param>
        /// <returns>The endpoint route builder for method chaining</returns>
        public static IEndpointRouteBuilder MapStandaloneServerApiEndPoints(this IEndpointRouteBuilder builder, IServer server)
        {
            builder.MapRtmpStreamManagerApiEndpoints(server);
            return builder;
        }

        /// <summary>
        /// Maps all standalone RTMP server API endpoints using the server instance from service provider.
        /// </summary>
        /// <param name="builder">The endpoint route builder</param>
        /// <returns>The endpoint route builder for method chaining</returns>
        public static IEndpointRouteBuilder MapStandaloneServerApiEndPoints(this IEndpointRouteBuilder builder)
        {
            var server = builder.ServiceProvider.GetRequiredService<IServer>();
            builder.MapRtmpStreamManagerApiEndpoints(server);
            return builder;
        }
    }
}
