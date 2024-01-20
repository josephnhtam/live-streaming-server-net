namespace LiveStreamingServerNet.Transmuxer.Internal.Contracts
{
    internal interface ITransmuxerEventDispatcher
    {
        Task TransmuxerStartedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
        Task TransmuxerStoppedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
