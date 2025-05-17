using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Represents a transcoding stream that provides functionalities
    /// to start, stop, and process media data transcoding.
    /// </summary>
    public interface ITranscodingStream : IAsyncDisposable
    {
        /// <summary>
        /// Asynchronously starts the transcoding process.
        /// </summary>
        ValueTask StartAsync();

        /// <summary>
        /// Asynchronously stops the transcoding process.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        ValueTask StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously writes a media buffer for transcoding.
        /// </summary>
        /// <param name="mediaType">The type of media data.</param>
        /// <param name="rentedBuffer">The buffer that contains the media data.</param>
        /// <param name="timestamp">The timestamp that indicates when the media data should be presented.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        ValueTask WriteAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);

        /// <summary>
        /// Occurs when transcoding has started.
        /// </summary>
        event EventHandler<TranscodingStartedEventArgs> TranscodingStarted;

        /// <summary>
        /// Occurs when transcoding has stopped.
        /// </summary>
        event EventHandler<TranscodingStoppedEventArgs> TranscodingStopped;

        /// <summary>
        /// Occurs when transcoding is canceled. 
        /// </summary>
        event EventHandler<TranscodingCanceledEventArgs> TranscodingCanceled;

        /// <summary>
        /// An asynchronous event handler that is triggered when a transcoded buffer is received.
        /// </summary>
        IAsyncEventHandler<TranscodedBufferReceivedEventArgs> TranscodedBufferReceived { get; }
    }

    /// <summary>
    /// Provides data for the TranscodingStarted event.
    /// </summary>
    /// <param name="Id">An identifier for the transcoding operation that has started.</param>
    public record struct TranscodingStartedEventArgs(int Id);

    /// <summary>
    /// Provides data for the TranscodingStopped event.
    /// </summary>
    /// <param name="Id">An identifier for the transcoding operation that has stopped.</param>
    public record struct TranscodingStoppedEventArgs(int Id);

    /// <summary>
    /// Provides data for the TranscodingCanceled event.
    /// </summary>
    /// <param name="Exception">An exception that caused the transcoding to be cancelled, if any.</param>
    public record struct TranscodingCanceledEventArgs(Exception? Exception);

    /// <summary>
    /// Provides data for the TranscodedBufferReceived asynchronous event.
    /// </summary>
    /// <param name="RentedBuffer">The rented buffer containing the transcoded media data.</param>
    public record struct TranscodedBufferReceivedEventArgs(IRentedBuffer RentedBuffer);
}
