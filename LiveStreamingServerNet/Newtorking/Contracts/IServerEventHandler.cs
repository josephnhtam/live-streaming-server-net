using LiveStreamingServerNet.Networking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IServerEventHandler
    {
        void OnListenerCreated(TcpListener tcpListener);
        void OnClientAccepted(TcpClient tcpClient);
        void OnClientPeerConnected(IClientPeer clientPeer);
        void OnClientPeerDisconnected(IClientPeer clientPeer);
        void OnServerStarted();
    }
}
