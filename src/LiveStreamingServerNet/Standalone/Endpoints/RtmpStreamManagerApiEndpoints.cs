using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Dtos;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone.Endpoints
{
    public static class RtmpStreamManagerApiEndpoints
    {
        public static IEndpointRouteBuilder MapRtmpStreamManagerApiEndpoints(this IEndpointRouteBuilder builder, IServer server)
        {
            var group = builder.MapGroup("api/v1/streams");

            group.MapGet("/", GetStreams(server));

            return builder;
        }

        public static Delegate GetStreams(IServer server) =>
            Ok<GetStreamsResponse> ([AsParameters] GetStreamsRequest request) =>
            {
                var apiService = server.Services.GetRequiredService<IRtmpStreamManagerApiService>();
                return TypedResults.Ok(apiService.GetStreams(request));
            };
    }
}
