using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts
{
    internal interface ISubtitleTranscriber : IAsyncDisposable
    {
        string Name { get; }
        Guid ContextIdentifier { get; }
        string StreamPath { get; }
        string SubtitleManifestPath { get; }
        SubtitleTrackOptions Options { get; }
        ValueTask StartAsync();
        ValueTask StopAsync();
        ValueTask EnqueueAudioBufferAsync(IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask ClearExpiredSegmentsAsync(uint oldestTimestamp);
        List<SeqSegment> GetSegments();
    }
}
