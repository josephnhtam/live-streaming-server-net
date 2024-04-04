namespace LiveStreamingServerNet.Transmuxer.Internal.Contracts
{
    internal interface ITransmuxerEventDispatcher
    {
        Task TransmuxerStartedAsync(string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task TransmuxerStoppedAsync(string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
