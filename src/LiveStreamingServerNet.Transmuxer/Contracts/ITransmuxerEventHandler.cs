namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerEventHandler
    {
        Task OnTransmuxerStartedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
        Task OnTransmuxerStoppedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
