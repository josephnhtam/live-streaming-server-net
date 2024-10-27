using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    /// <summary>
    /// Interface providing basic session information.
    /// </summary>
    public interface ISessionInfo
    {
        /// <summary>
        /// Unique identifier for the session.
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Indicates whether the session is currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Time when the session was established.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Local endpoint information for the session.
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Remote endpoint information for the session.
        /// </summary>
        EndPoint RemoteEndPoint { get; }
    }

    /// <summary>
    /// Interface for controlling session state.
    /// </summary>
    public interface ISessionControl : ISessionInfo
    {
        /// <summary>
        /// Disconnects the session.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Disconnects the session and waits for session cleanup.
        /// </summary>
        /// <param name="cancellation">Token to cancel waiting for the cleanup process.</param>
        Task DisconnectAsync(CancellationToken cancellation = default);
    }

    /// <summary>
    /// Interface for handling session operations including data transmission.
    /// </summary>
    public interface ISessionHandle : ISessionControl
    {
        /// <summary>
        /// Enqueues data buffer for asynchronous transmission.
        /// The callback is invoked after the data has been written to the socket's send buffer or when a failure occurs.
        /// </summary>
        /// <param name="dataBuffer">Buffer containing data to transmit.</param>
        /// <param name="callback">Optional callback invoked with true on successful socket buffer write, false on failure.</param>
        void Send(IDataBuffer dataBuffer, Action<bool>? callback = null);

        /// <summary>
        /// Enqueues rented buffer for asynchronous transmission.
        /// The callback is invoked after the data has been written to the socket's send buffer or when a failure occurs.
        /// </summary>
        /// <param name="rentedBuffer">Rented buffer containing data to transmit.</param>
        /// <param name="callback">Optional callback invoked with true on successful socket buffer write, false on failure.</param>
        void Send(IRentedBuffer rentedBuffer, Action<bool>? callback = null);

        /// <summary>
        /// Creates and enqueues data for asynchronous transmission using a writer function.
        /// The callback is invoked after the data has been written to the socket's send buffer or when a failure occurs.
        /// </summary>
        /// <param name="writer">Function to populate the data buffer with content to transmit.</param>
        /// <param name="callback">Optional callback invoked with true on successful socket buffer write, false on failure.</param>
        void Send(Action<IDataBuffer> writer, Action<bool>? callback = null);

        /// <summary>
        /// Enqueues data buffer and asynchronously waits until it is written to the socket's send buffer.
        /// </summary>
        /// <param name="dataBuffer">Buffer containing data to transmit.</param>
        /// <returns>ValueTask that completes when data is written to the socket's send buffer.</returns>
        ValueTask SendAsync(IDataBuffer dataBuffer);

        /// <summary>
        /// Enqueues rented buffer and asynchronously waits until it is written to the socket's send buffer.
        /// </summary>
        /// <param name="rentedBuffer">Rented buffer containing data to transmit.</param>
        /// <returns>ValueTask that completes when data is written to the socket's send buffer.</returns>
        ValueTask SendAsync(IRentedBuffer rentedBuffer);

        /// <summary>
        /// Creates and enqueues data using a writer function and asynchronously waits until it is written to the socket's send buffer.
        /// </summary>
        /// <param name="writer">Function to populate the data buffer with content to transmit.</param>
        /// <returns>ValueTask that completes when data is written to the socket's send buffer.</returns>
        ValueTask SendAsync(Action<IDataBuffer> writer);
    }
}
