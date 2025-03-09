using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface ITranscodingStream : IAsyncDisposable
    {
        ValueTask StartAsync();
        ValueTask StopAsync(CancellationToken cancellationToken);
        ValueTask WriteAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);

        event EventHandler<TranscodingStartedEventArgs> TranscodingStarted;
        event EventHandler<TranscodingStoppedEventArgs> TranscodingStopped;
        event EventHandler<TranscodingCanceledEventArgs> TranscodingCanceled;
        IAsyncEventHandler<TranscodedBufferReceivedEventArgs> TranscodedBufferReceived { get; }
    }

    public record struct TranscodingStartedEventArgs(int Id);
    public record struct TranscodingStoppedEventArgs(int Id);
    public record struct TranscodingCanceledEventArgs(Exception? Exception);
    public record struct TranscodedBufferReceivedEventArgs(IRentedBuffer RentedBuffer);
}
