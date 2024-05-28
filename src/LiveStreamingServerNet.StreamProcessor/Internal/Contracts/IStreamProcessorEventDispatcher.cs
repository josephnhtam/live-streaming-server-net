namespace LiveStreamingServerNet.StreamProcessor.Internal.Contracts
{
    internal interface IStreamProcessorEventDispatcher
    {
        Task StreamProcssorStartedAsync(string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task StreamProcessorStoppedAsync(string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
