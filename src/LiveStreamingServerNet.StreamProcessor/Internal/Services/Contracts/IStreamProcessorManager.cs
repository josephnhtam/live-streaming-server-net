namespace LiveStreamingServerNet.StreamProcessor.Internal.Services.Contracts
{
    internal interface IStreamProcessorManager : IAsyncDisposable
    {
        Task StartProcessingStreamAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task StopProcessingStreamAsync(uint clientId, string streamPath);
    }
}
