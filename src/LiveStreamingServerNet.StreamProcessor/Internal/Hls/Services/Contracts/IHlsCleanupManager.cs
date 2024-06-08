namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts
{
    internal interface IHlsCleanupManager
    {
        ValueTask ExecuteCleanupAsync(string manifestPath);
        ValueTask ScheduleCleanupAsync(string manifestPath, IList<string> files, TimeSpan cleanupDelay);
    }
}
