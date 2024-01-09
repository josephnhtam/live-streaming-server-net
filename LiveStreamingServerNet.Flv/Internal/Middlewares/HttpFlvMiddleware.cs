using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Internal.Middlewares
{
    internal class HttpFlvMiddleware
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly RequestDelegate _next;

        public HttpFlvMiddleware(IServer server, RequestDelegate next)
        {
            _streamManager = server.Services.GetRequiredService<IFlvStreamManagerService>();
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cancellationToken = context.RequestAborted;

            var streamPath = context.Request.Path.ToString();

            if (_streamManager.IsStreamPathPublishing(streamPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
