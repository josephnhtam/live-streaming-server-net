using LiveStreamingServerNet.AdminPanelUI.Dtos;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.EndpointFilters;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Standalone.Endpoints
{
    public static class RtmpStreamManagerApiEndpoints
    {
        public static IEndpointRouteBuilder MapRtmpStreamManagerApiEndpoints(this IEndpointRouteBuilder builder, IServer server)
        {
            var group = builder
                .MapGroup("api/v1/streams")
                .AddEndpointFilter<ApiExceptionEndpointFilter>();

            group.MapGet("/", GetStreams(server));
            group.MapDelete("/", DeleteStream(server));

            return builder;
        }

        public static Delegate GetStreams(IServer server) =>
            async Task<Ok<GetStreamsResponse>> ([AsParameters] GetStreamsRequest request) =>
            {
                var apiService = server.Services.GetRequiredService<IRtmpStreamManagerApiService>();
                return TypedResults.Ok(await apiService.GetStreamsAsync(request));
            };

        public static Delegate DeleteStream(IServer server) =>
            async Task<Ok> ([FromQuery] string streamId) =>
            {
                var apiService = server.Services.GetRequiredService<IRtmpStreamManagerApiService>();
                await apiService.DeleteStreamAsync(streamId);
                return TypedResults.Ok();
            };
    }
}
