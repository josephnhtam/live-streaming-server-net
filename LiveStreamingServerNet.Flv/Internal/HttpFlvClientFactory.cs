using LiveStreamingServerNet.Flv.Internal.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class HttpFlvClientFactory : IHttpFlvClientFactory
    {
        private readonly IServiceProvider _services;
        private uint _lastClientId = 0;

        public HttpFlvClientFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IFlvClient CreateClient(HttpContext context, string streamPath, CancellationToken stoppingToken)
        {
            var clientId = Interlocked.Increment(ref _lastClientId);
            var client = _services.GetRequiredService<IFlvClient>();

            var streamWriter = new HttpResponseStreamWriter(context.Response);
            client.Initialize(clientId, streamPath, streamWriter, stoppingToken);

            return client;
        }
    }
}
