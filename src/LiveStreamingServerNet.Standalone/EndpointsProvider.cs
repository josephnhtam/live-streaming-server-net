using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone
{
    public static class EndpointsProvider
    {
        public static IEndpointRouteBuilder MapStandaloneServerApiEndPoints(this IEndpointRouteBuilder builder, IServer server)
        {
            builder.MapRtmpStreamManagerApiEndpoints(server);
            return builder;
        }

        public static IEndpointRouteBuilder MapStandaloneServerApiEndPoints(this IEndpointRouteBuilder builder)
        {
            var server = builder.ServiceProvider.GetRequiredService<IServer>();
            builder.MapRtmpStreamManagerApiEndpoints(server);
            return builder;
        }
    }
}
