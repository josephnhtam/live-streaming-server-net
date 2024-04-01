using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerEventHandler
    {
        int GetOrder() => 0;
        Task OnTransmuxerStartedAsync(IEventContext context, uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task OnTransmuxerStoppedAsync(IEventContext context, uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
