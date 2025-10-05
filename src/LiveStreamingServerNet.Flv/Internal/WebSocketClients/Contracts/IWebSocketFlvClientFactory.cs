using LiveStreamingServerNet.Flv.Internal.Contracts;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts
{
    internal interface IWebSocketFlvClientFactory
    {
        IFlvClient CreateClient(HttpContext context, WebSocket webSocket, string streamPath, IReadOnlyDictionary<string, string> streamArguments, CancellationToken stoppingToken);
    }
}
