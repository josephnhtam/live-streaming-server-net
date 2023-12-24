using LiveStreamingServer.Networking.Contracts;

namespace LiveStreamingServer.Newtorking.Contracts
{
    public interface IClientPeerHandler
    {
        void Initialize(IClientPeerHandle clientPeer);
        Task<bool> HandleClientPeerLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken);
    }
}
