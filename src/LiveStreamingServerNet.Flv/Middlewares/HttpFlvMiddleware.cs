using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Extensions;
using LiveStreamingServerNet.Flv.Internal.HttpClients.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Middlewares
{
    public class HttpFlvMiddleware
    {
        private readonly IHttpFlvClientFactory _clientFactory;
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvClientHandler _clientHandler;

        private readonly IStreamPathResolver _streamPathResolver;
        private readonly Func<FlvStreamContext, Task<bool>>? _onPrepareResponse;

        private readonly RequestDelegate _next;

        public HttpFlvMiddleware(IServer server, HttpFlvOptions options, RequestDelegate next)
        {
            _clientFactory = server.Services.GetRequiredService<IHttpFlvClientFactory>();
            _streamManager = server.Services.GetRequiredService<IFlvStreamManagerService>();
            _clientHandler = server.Services.GetRequiredService<IFlvClientHandler>();
            _streamPathResolver = options.StreamPathResolver ?? new DefaultStreamPathResolver();
            _onPrepareResponse = options.OnPrepareResponse;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest ||
                !context.ValidateNoEndpointDelegate() ||
                !context.ValidateGetOrHeadMethod() ||
                !_streamPathResolver.ResolveStreamPathAndArguments(context, out var streamPath, out var streamArguments))
            {
                await _next.Invoke(context);
                return;
            }

            WriteResponseHeader(context);

            if (_onPrepareResponse != null && !await _onPrepareResponse(new FlvStreamContext(context, streamPath, streamArguments.AsReadOnly())))
                return;

            await TryServeHttpFlv(context, streamPath, streamArguments.AsReadOnly());
        }

        private static void WriteResponseHeader(HttpContext context)
        {
            context.Response.ContentType = "video/x-flv";
        }

        private async Task TryServeHttpFlv(HttpContext context, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!_streamManager.IsStreamPathPublishing(streamPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await SubscribeToStreamAsync(context, streamPath, streamArguments);
        }

        private IFlvClient CreateClient(HttpContext context, string streamPath, CancellationToken cancellation)
        {
            return _clientFactory.CreateClient(context, streamPath, cancellation);
        }

        private async Task SubscribeToStreamAsync(HttpContext context, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var cancellation = context.RequestAborted;

            await using var client = CreateClient(context, streamPath, cancellation);

            switch (_streamManager.StartSubscribingStream(client, streamPath))
            {
                case SubscribingStreamResult.Succeeded:
                    await _clientHandler.RunClientAsync(client);
                    return;
                case SubscribingStreamResult.StreamDoesntExist:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                case SubscribingStreamResult.AlreadySubscribing:
                    throw new InvalidOperationException("Already subscribing");
            }
        }
    }
}
