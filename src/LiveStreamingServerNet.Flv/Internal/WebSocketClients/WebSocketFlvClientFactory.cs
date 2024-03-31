using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts;
using System.Net.WebSockets;

namespace LiveStreamingServerNet.Flv.Internal.WebSocketClients
{
    internal class WebSocketFlvClientFactory : IWebSocketFlvClientFactory
    {
        private readonly IFlvClientFactory _flvClientFactory;
        private uint _lastClientId = 0;

        public WebSocketFlvClientFactory(IFlvClientFactory flvClientFactory)
        {
            _flvClientFactory = flvClientFactory;
        }

        public IFlvClient CreateClient(WebSocket webSocket, string streamPath, CancellationToken stoppingToken)
        {
            var clientId = $"WS-{Interlocked.Increment(ref _lastClientId)}";
            var streamWriter = new WebSocketStreamWriter(webSocket);
            return _flvClientFactory.Create(clientId, streamPath, streamWriter, stoppingToken);
        }
    }
}
