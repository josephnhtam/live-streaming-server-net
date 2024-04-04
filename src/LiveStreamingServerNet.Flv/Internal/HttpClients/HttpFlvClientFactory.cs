using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.HttpClients.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Internal.HttpClients
{
    internal class HttpFlvClientFactory : IHttpFlvClientFactory
    {
        private readonly IFlvClientFactory _flvClientFactory;
        private uint _lastClientId = 0;

        public HttpFlvClientFactory(IFlvClientFactory flvClientFactory)
        {
            _flvClientFactory = flvClientFactory;
        }

        public IFlvClient CreateClient(HttpContext context, string streamPath, CancellationToken stoppingToken)
        {
            var clientId = $"HTTP-{Interlocked.Increment(ref _lastClientId)}";
            var streamWriter = new HttpResponseStreamWriter(context.Response);
            return _flvClientFactory.Create(clientId, streamPath, streamWriter, stoppingToken);
        }
    }
}
