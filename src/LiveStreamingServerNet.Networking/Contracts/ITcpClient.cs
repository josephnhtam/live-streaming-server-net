using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    /// <summary>
    /// Interface defining TCP client socket configuration and state.
    /// </summary>
    public interface ITcpClient
    {
        /// <summary>
        /// Gets number of bytes available in receive buffer.
        /// </summary>
        int Available { get; }

        /// <summary>
        /// Gets connection state of socket.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets or sets size of receive buffer in bytes.
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Gets or sets size of send buffer in bytes.
        /// </summary>
        int SendBufferSize { get; set; }

        /// <summary>
        /// Gets or sets receive operation timeout in milliseconds.
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets send operation timeout in milliseconds.
        /// </summary>
        int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets whether Nagle's algorithm is disabled.
        /// True disables Nagle's algorithm for send coalescing.
        /// </summary>
        bool NoDelay { get; set; }

        /// <summary>
        /// Gets or sets socket linger options for graceful close.
        /// </summary>
        [DisallowNull] LingerOption? LingerState { get; set; }
    }
}
