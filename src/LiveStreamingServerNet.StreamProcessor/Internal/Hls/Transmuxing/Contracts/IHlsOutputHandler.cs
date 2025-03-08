using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts
{
    internal interface IHlsOutputHandler
    {
        ValueTask AddSegmentAsync(TsSegment segment);
        ValueTask ExecuteCleanupAsync();
        ValueTask ScheduleCleanupAsync();
    }
}
