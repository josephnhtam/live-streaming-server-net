using LiveStreamingServerNet.Flv.Contracts;
using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Configurations
{
    /// <summary>
    /// Configuration options for WebSocket FLV streaming.
    /// </summary>
    public class WebSocketFlvOptions
    {
        /// <summary>
        /// Resolves stream paths for WebSocket FLV requests.
        /// If not set, default path resolution will be used.
        /// </summary>
        public IStreamPathResolver? StreamPathResolver { get; set; }

        /// <summary>
        /// Context for accepting WebSocket connections.
        /// Allows configuration of WebSocket-specific options like subprotocols and buffer sizes.
        /// </summary>
        public WebSocketAcceptContext? WebSocketAcceptContext { get; set; }

        /// <summary>
        /// Callback function executed before establishing the WebSocket connection.
        /// Returns true to accept the connection, false to reject.
        /// Allows for custom validation and connection preparation.
        /// </summary>
        public Func<FlvStreamContext, Task<bool>>? OnPrepareResponse { get; set; }
    }
}
