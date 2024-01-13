using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;

namespace LiveStreamingServerNet.Flv.Internal.WebSocketClients
{
    internal class WebSocketFlvClientFactory : IWebSocketFlvClientFactory
    {
        private readonly IServiceProvider _services;
        private uint _lastClientId = 0;

        public WebSocketFlvClientFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IFlvClient CreateClient(WebSocket webSocket, string streamPath, CancellationToken stoppingToken)
        {
            var clientId = $"WS-{Interlocked.Increment(ref _lastClientId)}";
            var client = _services.GetRequiredService<IFlvClient>();

            var streamWriter = new WebSocketStreamWriter(webSocket);
            client.Initialize(clientId, streamPath, streamWriter, stoppingToken);

            return client;
        }
    }
}
