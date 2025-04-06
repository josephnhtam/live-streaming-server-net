using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts
{
    internal interface IHlsOutputHandler : IAsyncDisposable
    {
        string Name { get; }
        Guid ContextIdentifier { get; }
        string StreamPath { get; }

        ValueTask InitializeAsync();
        ValueTask CompleteAsync();
        ValueTask AddSegmentAsync(SeqSegment segment);
        ValueTask ExecuteCleanupAsync();
        ValueTask ScheduleCleanupAsync();
    }
}
