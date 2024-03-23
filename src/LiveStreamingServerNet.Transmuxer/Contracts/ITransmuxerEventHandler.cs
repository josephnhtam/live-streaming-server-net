namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerEventHandler
    {
        int GetOrder() => 0;
        Task OnTransmuxerStartedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
        Task OnTransmuxerStoppedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments);
    }
}
