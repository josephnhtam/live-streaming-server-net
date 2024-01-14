using LiveStreamingServerNet.Flv.Internal.Contracts;
using System.Net.WebSockets;

namespace LiveStreamingServerNet.Flv.Internal.WebSocketClients
{
    internal class WebSocketStreamWriter : IStreamWriter
    {
        private readonly WebSocket _webSocket;

        public WebSocketStreamWriter(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken);
        }
    }
}
