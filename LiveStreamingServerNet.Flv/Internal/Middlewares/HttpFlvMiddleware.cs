using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Internal.Middlewares
{
    internal class HttpFlvMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpFlvMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next.Invoke(context);
        }
    }
}
