using LiveStreamingServer.Networking.Contracts;

namespace LiveStreamingServer.Newtorking.Contracts
{
    public interface IClientPeerHandlerFactory
    {
        IClientPeerHandler CreateClientPeerHandler(IClientPeerHandle clientPeer);
    }
}
