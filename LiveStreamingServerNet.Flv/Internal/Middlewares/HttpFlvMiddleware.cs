using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Internal.Middlewares
{
    internal class HttpFlvMiddleware
    {
        private readonly IHttpFlvClientFactory _clientFactory;
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvHeaderWriter _headerWriter;
        private readonly RequestDelegate _next;

        public HttpFlvMiddleware(IServer server, IFlvHeaderWriter headerWriter, RequestDelegate next)
        {
            _clientFactory = server.Services.GetRequiredService<IHttpFlvClientFactory>();
            _streamManager = server.Services.GetRequiredService<IFlvStreamManagerService>();
            _headerWriter = headerWriter;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!GetStreamPathAndArguments(context, out var streamPath, out var streamArguments))
            {
                await _next.Invoke(context);
                return;
            }

            if (!_streamManager.IsStreamPathPublishing(streamPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await SubscribeToStreamAsync(context, streamPath, streamArguments);
        }

        private async Task SubscribeToStreamAsync(HttpContext context, string streamPath, IDictionary<string, string> streamArguments)
        {
            await WriteFlvHeaderAsync(context, streamPath, streamArguments, context.RequestAborted);

            await using var client = CreateClient(context);
            switch (_streamManager.StartSubscribingStream(client, streamPath))
            {
                case SubscribingStreamResult.Succeeded:
                    await client.UntilComplete();
                    _streamManager.StopSubscribingStream(client);
                    return;
                case SubscribingStreamResult.StreamDoesntExist:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                case SubscribingStreamResult.AlreadySubscribing:
                    throw new InvalidOperationException("Already subscribing");
            }
        }

        private IFlvClient CreateClient(HttpContext context)
        {
            return _clientFactory.CreateClient(context, context.RequestAborted);
        }

        private async Task WriteFlvHeaderAsync(HttpContext context, string streamPath, IDictionary<string, string> streamArguments, CancellationToken cancellation)
        {
            await _headerWriter.WriteHeaderAsync(context, streamPath, streamArguments, cancellation);
        }

        private static bool GetStreamPathAndArguments(HttpContext context, out string streamPath, out IDictionary<string, string> streamArguments)
        {
            streamPath = default!;
            streamArguments = default!;

            var path = context.Request.Path.ToString();
            var query = context.Request.QueryString.ToString();

            if (path.Length <= 1)
                return false;

            streamPath = path.TrimEnd('/');
            streamArguments = QueryHelpers.ParseQuery(query).ToDictionary(x => x.Key, x => x.Value.ToString());
            return true;
        }
    }
}
