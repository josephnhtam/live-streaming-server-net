using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface IStreamProcessorFactory
    {
        Task<IStreamProcessor?> CreateAsync(IClientHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
