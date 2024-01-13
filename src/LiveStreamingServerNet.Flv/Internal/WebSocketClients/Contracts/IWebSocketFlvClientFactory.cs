using LiveStreamingServerNet.Flv.Internal.Contracts;
using System.Net.WebSockets;

namespace LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts
{
    internal interface IWebSocketFlvClientFactory
    {
        IFlvClient CreateClient(WebSocket webSocket, string streamPath, CancellationToken stoppingToken);
    }
}
