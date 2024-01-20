namespace LiveStreamingServerNet.Transmuxer.Internal.Contracts
{
    internal interface ITransmuxerEventDispatcher
    {
        Task TransmuxerStartedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
        Task TransmuxerStoppedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
