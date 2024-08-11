namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface ISessionHandler : IAsyncDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
        Task<bool> HandleSessionLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken);
    }
}
