using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IClientHandler : IAsyncDisposable
    {
        Task InitializeAsync(IClientHandle client);
        Task<bool> HandleClientLoopAsync(ReadOnlyStream networkStream, CancellationToken cancellationToken);
    }
}
