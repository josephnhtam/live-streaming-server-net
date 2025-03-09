using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts
{
    public interface ITranscriptionStream : IAsyncDisposable
    {
        ValueTask StartAsync();
        ValueTask StopAsync(CancellationToken cancellationToken);
        ValueTask SendAsync(IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);

        event EventHandler<TranscriptingStartedEventArgs> TranscriptingStarted;
        event EventHandler<TranscriptingStoppedEventArgs> TranscriptingStopped;
        event EventHandler<TranscriptingCanceledEventArgs> TranscriptingCanceled;
        event EventHandler<TranscriptionResultReceivedEventArgs> TranscriptionResultReceived;
    }

    public record struct TranscriptingStartedEventArgs(int Id);
    public record struct TranscriptingStoppedEventArgs(int Id);
    public record struct TranscriptingCanceledEventArgs(Exception? Exception);
    public record struct TranscriptionResultReceivedEventArgs(TranscriptionResult Result);
}
