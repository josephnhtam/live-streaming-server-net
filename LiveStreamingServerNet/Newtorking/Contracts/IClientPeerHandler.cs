namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IClientPeerHandler : IAsyncDisposable
    {
        Task<bool> HandleClientPeerLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken);
    }
}
