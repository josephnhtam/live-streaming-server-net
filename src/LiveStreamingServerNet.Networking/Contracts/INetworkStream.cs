using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Networking.Contracts
{
    /// <summary>
    /// Interface for network stream operations.
    /// </summary>
    public interface INetworkStream : INetworkStreamWriter, INetworkStreamReader, IAsyncDisposable
    {
        /// <summary>
        /// Gets the underlying stream used for network I/O operations.
        /// </summary>
        Stream InnerStream { get; }
    }

    /// <summary>
    /// Interface for reading data from a network stream.
    /// </summary>
    public interface INetworkStreamReader : IStreamReader { }

    /// <summary>
    /// Interface for writing data to a network stream.
    /// </summary>
    public interface INetworkStreamWriter
    {
        /// <summary>
        /// Asynchronously writes data to the network stream.
        /// </summary>
        /// <param name="buffer">The byte array containing the data to write.</param>
        /// <param name="offset">The zero-based position in the buffer at which to begin writing.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A ValueTask representing the asynchronous write operation.</returns>
        ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }
}
