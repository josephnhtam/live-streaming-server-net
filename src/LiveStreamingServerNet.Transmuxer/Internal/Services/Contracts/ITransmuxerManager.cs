namespace LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts
{
    internal interface ITransmuxerManager : IAsyncDisposable
    {
        Task StartRemuxingStreamAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task StopRemuxingStreamAsync(uint clientId, string streamPath);
    }
}
