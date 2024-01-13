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
        private readonly IFlvMediaTagManagerService _mediaTagManager;
        private readonly IHttpFlvHeaderWriter _headerWriter;
        private readonly RequestDelegate _next;

        public HttpFlvMiddleware(IServer server, IHttpFlvHeaderWriter headerWriter, RequestDelegate next)
        {
            _clientFactory = server.Services.GetRequiredService<IHttpFlvClientFactory>();
            _streamManager = server.Services.GetRequiredService<IFlvStreamManagerService>();
            _mediaTagManager = server.Services.GetRequiredService<IFlvMediaTagManagerService>();
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
            var cancellation = context.RequestAborted;

            await WriteHttpFlvHeaderAsync(context, streamPath, streamArguments, cancellation);

            await using var client = CreateClient(context, streamPath, cancellation);
            switch (_streamManager.StartSubscribingStream(client, streamPath))
            {
                case SubscribingStreamResult.Succeeded:
                    await RunClientAsync(client, cancellation);
                    return;
                case SubscribingStreamResult.StreamDoesntExist:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                case SubscribingStreamResult.AlreadySubscribing:
                    throw new InvalidOperationException("Already subscribing");
            }
        }

        private async Task RunClientAsync(IFlvClient client, CancellationToken cancellationToken)
        {
            try
            {
                await SendFlvHeaderAsync(client, cancellationToken);
                await SendCachedFlvTagsAsync(client, cancellationToken);

                client.CompleteInitialization();
                await client.UntilComplete();
            }
            finally
            {
                _streamManager.StopSubscribingStream(client);
            }
        }

        private static async Task SendFlvHeaderAsync(IFlvClient client, CancellationToken cancellationToken)
        {
            await client.FlvWriter.WriteHeaderAsync(true, true, cancellationToken);
        }

        private async Task SendCachedFlvTagsAsync(IFlvClient client, CancellationToken cancellationToken)
        {
            var streamContext = _streamManager.GetFlvStreamContext(client.StreamPath)!;

            await _mediaTagManager.SendCachedHeaderTagsAsync(client, streamContext, 0, cancellationToken);
            await _mediaTagManager.SendCachedGroupOfPicturesTagsAsync(client, streamContext, cancellationToken);
        }

        private IFlvClient CreateClient(HttpContext context, string streamPath, CancellationToken cancellation)
        {
            return _clientFactory.CreateClient(context, streamPath, cancellation);
        }

        private async Task WriteHttpFlvHeaderAsync(HttpContext context, string streamPath, IDictionary<string, string> streamArguments, CancellationToken cancellation)
        {
            await _headerWriter.WriteHeaderAsync(context, streamPath, streamArguments, cancellation);
        }

        private static bool GetStreamPathAndArguments(HttpContext context, out string streamPath, out IDictionary<string, string> streamArguments)
        {
            streamPath = default!;
            streamArguments = default!;

            var path = context.Request.Path.ToString().TrimEnd('/');
            var query = context.Request.QueryString.ToString();
            var extension = Path.GetExtension(path);

            if (path.Length <= 1 || !extension.Equals(".flv", StringComparison.InvariantCultureIgnoreCase))
                return false;

            streamPath = path.Substring(0, path.Length - 4);
            streamArguments = QueryHelpers.ParseQuery(query).ToDictionary(x => x.Key, x => x.Value.ToString());
            return true;
        }
    }
}
