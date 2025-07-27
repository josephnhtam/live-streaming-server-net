using LiveStreamingServerNet.AdminPanelUI.Dtos;
using LiveStreamingServerNet.Networking.Server.Contracts;
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
    /// <summary>
    /// Defines API endpoints for managing RTMP streams.
    /// </summary>
    public static class RtmpStreamManagerApiEndpoints
    {
        /// <summary>
        /// Maps RTMP stream management endpoints to the specified route builder.
        /// </summary>
        /// <param name="builder">The endpoint route builder</param>
        /// <param name="server">The server instance</param>
        /// <returns>The endpoint route builder for method chaining</returns>
        public static IEndpointRouteBuilder MapRtmpStreamManagerApiEndpoints(this IEndpointRouteBuilder builder, IServer server)
        {
            var group = builder
                .MapGroup("api/v1/streams")
                .AddEndpointFilter<ApiExceptionEndpointFilter>();

            group.MapGet("/", GetStreams(server));
            group.MapDelete("/", DeleteStream(server));

            return builder;
        }

        /// <summary>
        /// Creates an endpoint handler that retrieves active streams.
        /// </summary>
        /// <param name="server">The server instance</param>
        /// <returns>An endpoint handler that returns stream information</returns>
        public static Delegate GetStreams(IServer server) =>
            async Task<Ok<GetStreamsResponse>> ([AsParameters] GetStreamsRequest request) =>
            {
                var apiService = server.Services.GetRequiredService<IRtmpStreamManagerApiService>();
                return TypedResults.Ok(await apiService.GetStreamsAsync(request).ConfigureAwait(false));
            };

        /// <summary>
        /// Creates an endpoint handler that deletes a stream.
        /// </summary>
        /// <param name="server">The server instance</param>
        /// <returns>An endpoint handler that deletes the specified stream</returns>
        public static Delegate DeleteStream(IServer server) =>
            async Task<Ok> ([FromQuery] string streamId) =>
            {
                var apiService = server.Services.GetRequiredService<IRtmpStreamManagerApiService>();
                await apiService.DeleteStreamAsync(streamId).ConfigureAwait(false);
                return TypedResults.Ok();
            };
    }
}
