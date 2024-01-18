namespace LiveStreamingServerNet.Transmuxer.Internal.Contracts
{
    internal interface ITransmuxerEventDispatcher
    {
        Task TransmuxerStartedAsync(string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
        Task TransmuxerStoppedAsync(string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
