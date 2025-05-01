using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts
{
    /// <summary>
    /// Represents a transcription stream that provides functionalities to start, stop,
    /// and process the transcription of media data.
    /// </summary>
    public interface ITranscriptionStream : IAsyncDisposable
    {
        /// <summary>
        /// Asynchronously starts the transcription process.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask StartAsync();

        /// <summary>
        /// Asynchronously stops the transcription process.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests while stopping the transcription process.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously writes a media buffer to be transcribed.
        /// </summary>
        /// <param name="rentedBuffer">The buffer containing the media data to be transcribed.</param>
        /// <param name="timestamp">The timestamp that indicates when the media data should be processed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests while writing to the transcription process.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask WriteAsync(IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);

        /// <summary>
        /// Occurs when the transcription process has successfully started.
        /// </summary>
        event EventHandler<TranscriptionStartedEventArgs> TranscriptionStarted;

        /// <summary>
        /// Occurs when the transcription process has stopped.
        /// </summary>
        event EventHandler<TranscriptionStoppedEventArgs> TranscriptionStopped;

        /// <summary>
        /// Occurs when the transcription process is canceled.
        /// </summary>
        event EventHandler<TranscriptionCanceledEventArgs> TranscriptionCanceled;

        /// <summary>
        /// Occurs when a transcription result is currently being processed.
        /// </summary>
        event EventHandler<TranscribingResultReceivedEventArgs> TranscribingResultReceived;

        /// <summary>
        /// Occurs when the final transcription result is received.
        /// </summary>
        event EventHandler<TranscribedResultReceivedEventArgs> TranscribedResultReceived;
    }

    /// <summary>
    /// Provides data for the <see cref="ITranscriptionStream.TranscriptionStarted"/> event.
    /// </summary>
    /// <param name="Id">An identifier for the transcription operation that has started.</param>
    public record struct TranscriptionStartedEventArgs(int Id);

    /// <summary>
    /// Provides data for the <see cref="ITranscriptionStream.TranscriptionStopped"/> event.
    /// </summary>
    /// <param name="Id">An identifier for the transcription operation that has stopped.</param>
    public record struct TranscriptionStoppedEventArgs(int Id);

    /// <summary>
    /// Provides data for the <see cref="ITranscriptionStream.TranscriptionCanceled"/> event.
    /// </summary>
    /// <param name="Exception">The exception that caused the transcription to be canceled, if any.</param>
    public record struct TranscriptionCanceledEventArgs(Exception? Exception);

    /// <summary>
    /// Provides data for the <see cref="ITranscriptionStream.TranscribingResultReceived"/> event.
    /// </summary>
    /// <param name="Result">The transcription result that is being processed.</param>
    public record struct TranscribingResultReceivedEventArgs(TranscriptionResult Result);

    /// <summary>
    /// Provides data for the <see cref="ITranscriptionStream.TranscribedResultReceived"/> event.
    /// </summary>
    /// <param name="Result">The final transcription result received after processing.</param>
    public record struct TranscribedResultReceivedEventArgs(TranscriptionResult Result);
}