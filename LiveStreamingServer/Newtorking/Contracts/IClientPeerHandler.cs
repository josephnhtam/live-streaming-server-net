namespace LiveStreamingServer.Newtorking.Contracts
{
    public interface IClientPeerHandler
    {
        Task<bool> HandleClientPeerLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken);
    }
}
