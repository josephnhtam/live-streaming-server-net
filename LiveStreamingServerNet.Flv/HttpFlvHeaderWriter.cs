using LiveStreamingServerNet.Flv.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv
{
    public class HttpFlvHeaderWriter : IHttpFlvHeaderWriter
    {
        public Task WriteHeaderAsync(HttpContext httpContext, string streamPath, IDictionary<string, string> streamArguments, CancellationToken cancellation)
        {
            var response = httpContext.Response;
            response.ContentType = "video/x-flv";
            //response.Headers["Transfer-Encoding"] = "chunked";
            //response.Headers["Cache-Control"] = "no-cache";
            //response.Headers["Connection"] = "keep-alive";

            return Task.CompletedTask;
        }
    }
}
