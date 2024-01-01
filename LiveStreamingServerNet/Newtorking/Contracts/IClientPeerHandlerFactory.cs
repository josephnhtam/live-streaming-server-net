using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IClientPeerHandlerFactory
    {
        IClientPeerHandler CreateClientPeerHandler(IClientPeerHandle clientPeer);
    }
}
