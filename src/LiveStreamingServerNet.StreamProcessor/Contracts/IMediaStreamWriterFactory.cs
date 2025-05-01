using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Defines a factory interface for creating instances of <see cref="IMediaStreamWriter"/>.
    /// </summary>
    public interface IMediaStreamWriterFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="IMediaStreamWriter"/> using the provided destination stream writer.
        /// </summary>
        /// <param name="dstStreamWriter">
        /// The destination stream writer where the media data will be written. This stream writer is used 
        /// by the media stream writer to output the processed media data.
        /// </param>
        IMediaStreamWriter Create(IStreamWriter dstStreamWriter);
    }
}
