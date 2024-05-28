using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface IStreamProcessorEventHandler
    {
        int GetOrder() => 0;
        Task OnStreamProcessorStartedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task OnStreamProcessorStoppedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
