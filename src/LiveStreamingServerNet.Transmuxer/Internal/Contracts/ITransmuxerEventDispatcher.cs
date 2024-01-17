namespace LiveStreamingServerNet.Transmuxer.Internal.Contracts
{
    internal interface ITransmuxerEventDispatcher
    {
        Task TransmuxerStartedAsync(string inputPath, string outputDirPath, string streamPath, IDictionary<string, string> streamArguments);
        Task TransmuxerStoppedAsync(string inputPath, string outputDirPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
