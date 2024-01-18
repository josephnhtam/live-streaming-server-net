namespace LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts
{
    internal interface ITransmuxerManager
    {
        Task StartRemuxingStreamAsync(uint clientId, string streamPath, IDictionary<string, string> streamArguments);
        Task StopRemuxingStreamAsync(uint clientId, string streamPath);
    }
}
