namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerEventHandler
    {
        Task OnTransmuxerStartedAsync(string inputPath, string outputDirPath, string streamPath, IDictionary<string, string> streamArguments);
        Task OnTransmuxerStoppedAsync(string inputPath, string outputDirPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
