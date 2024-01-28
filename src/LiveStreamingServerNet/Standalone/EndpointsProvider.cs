using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace LiveStreamingServerNet.Standalone
{
    public static class EndpointsProvider
    {
        public static IEndpointRouteBuilder MapStandaloneServerApiEndPoints(this IEndpointRouteBuilder builder, IServer server)
        {
            builder.MapRtmpStreamManagerApiEndpoints(server);
            return builder;
        }
    }
}
