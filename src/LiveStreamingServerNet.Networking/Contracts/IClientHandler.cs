namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IClientHandler : IAsyncDisposable
    {
        Task InitializeAsync(IClientHandle client);
        Task<bool> HandleClientLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken);
    }
}
