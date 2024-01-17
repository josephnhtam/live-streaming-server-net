namespace LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts
{
    internal interface ITransmuxerManager
    {
        Task StartRemuxingStreamAsync(string streamPath, IDictionary<string, string> streamArguments);
        Task StopRemuxingStreamAsync(string streamPath);
    }
}
