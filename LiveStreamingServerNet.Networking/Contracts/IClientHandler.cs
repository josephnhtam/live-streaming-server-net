namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IClientHandler : IAsyncDisposable
    {
        Task<bool> HandleClientLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken);
    }
}
