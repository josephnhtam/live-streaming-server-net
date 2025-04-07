using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts
{
    public interface ITranscriptionStream : IAsyncDisposable
    {
        ValueTask StartAsync();
        ValueTask StopAsync(CancellationToken cancellationToken);
        ValueTask SendAsync(IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);

        event EventHandler<TranscriptionStartedEventArgs> TranscriptionStarted;
        event EventHandler<TranscriptionStoppedEventArgs> TranscriptionStopped;
        event EventHandler<TranscriptionCanceledEventArgs> TranscriptionCanceled;
        event EventHandler<TranscribingResultReceivedEventArgs> TranscribingResultReceived;
        event EventHandler<TranscribedResultReceivedEventArgs> TranscribedResultReceived;
    }

    public record struct TranscriptionStartedEventArgs(int Id);
    public record struct TranscriptionStoppedEventArgs(int Id);
    public record struct TranscriptionCanceledEventArgs(Exception? Exception);
    public record struct TranscribingResultReceivedEventArgs(TranscribingResult Result);
    public record struct TranscribedResultReceivedEventArgs(TranscribedResult Result);
}
