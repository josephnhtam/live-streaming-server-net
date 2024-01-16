namespace LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts
{
    internal interface ITransmuxerManager
    {
        ValueTask StartRemuxingStreamAsync(string streamPath, IDictionary<string, string> streamArguments);
        ValueTask StopRemuxingStreamAsync(string streamPath);
    }
}
