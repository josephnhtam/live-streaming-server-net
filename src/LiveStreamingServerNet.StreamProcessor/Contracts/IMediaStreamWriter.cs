using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Defines a contract for a media stream writer that is responsible for writing media headers
    /// and media buffers asynchronously during streaming.
    /// </summary>
    public interface IMediaStreamWriter : IAsyncDisposable
    {
        /// <summary>
        /// Asynchronously writes the media header.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        ValueTask WriteHeaderAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously writes a buffer of media data.
        /// </summary>
        /// <param name="mediaType">The type of media.</param>
        /// <param name="rentedBuffer">The buffer that holds the media data.</param>
        /// <param name="timestamp">The timestamp for the media data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask WriteBufferAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);
    }
}
