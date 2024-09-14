namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IClientHandler : IAsyncDisposable
    {
        Task InitializeAsync();
        Task<bool> HandleClientLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken);
    }
}
