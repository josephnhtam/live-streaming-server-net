namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface ISessionHandler : IAsyncDisposable
    {
        ValueTask<bool> InitializeAsync(CancellationToken cancellationToken);
        Task<bool> HandleSessionLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken);
    }
}
