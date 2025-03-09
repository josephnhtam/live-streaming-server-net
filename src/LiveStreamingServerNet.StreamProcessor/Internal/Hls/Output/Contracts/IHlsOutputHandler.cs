using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts
{
    internal interface IHlsOutputHandler : IAsyncDisposable
    {
        string Name { get; }
        Guid ContextIdentifier { get; }
        string StreamPath { get; }

        ValueTask AddSegmentAsync(TsSegment segment);
        ValueTask ExecuteCleanupAsync();
        ValueTask ScheduleCleanupAsync();
        ValueTask InterceptMediaPacketAsync(MediaType mediaType, IRentedBuffer buffer, uint timestamp);
    }
}
