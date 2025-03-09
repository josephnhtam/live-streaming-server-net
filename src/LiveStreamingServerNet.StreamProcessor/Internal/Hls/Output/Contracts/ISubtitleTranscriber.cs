using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts
{
    internal interface ISubtitleTranscriber : IAsyncDisposable
    {
        SubtitleTrackOptions Options { get; }
        ValueTask StartAsync();
        ValueTask EnqueueAudioBufferAsync(IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask<TranscriptionResult> ReceiveTranscriptionResultAsync(CancellationToken cancellationToken);
    }
}
