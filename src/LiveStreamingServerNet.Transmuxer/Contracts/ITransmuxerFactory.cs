using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerFactory
    {
        Task<ITransmuxer> CreateAsync(IClientHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
