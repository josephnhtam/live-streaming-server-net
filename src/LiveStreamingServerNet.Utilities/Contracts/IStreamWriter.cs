namespace LiveStreamingServerNet.Utilities.Contracts
{
    /// <summary>
    /// Represents an abstraction for writing streams of data asynchronously.
    /// </summary>
    public interface IStreamWriter
    {
        /// <summary>
        /// Asynchronously writes a sequence of bytes to the underlying data stream.
        /// </summary>
        /// <param name="buffer">A read-only memory buffer containing the data to be written.</param>
        /// <param name="cancellationToken">A token that allows the asynchronous write operation to be canceled.</param>
        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    }
}