namespace LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts
{
    internal interface ITransmuxerManager : IAsyncDisposable
    {
        Task StartTransmuxingStreamAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task StopTransmuxingStreamAsync(uint clientId, string streamPath);
    }
}
