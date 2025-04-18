﻿using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts
{
    internal interface IHlsOutputHandler
    {
        string Name { get; }
        Guid ContextIdentifier { get; }
        string StreamPath { get; }

        ValueTask AddSegmentAsync(TsSegment segment);
        ValueTask ExecuteCleanupAsync();
        ValueTask ScheduleCleanupAsync();
    }
}
