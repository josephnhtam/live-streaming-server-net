using LiveStreamingServerNet.Networking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IServerEventHandler
    {
        Task OnListenerCreatedAsync(TcpListener tcpListener);
        Task OnClientAcceptedAsync(TcpClient tcpClient);
        Task OnClientPeerConnectedAsync(IClientPeer clientPeer);
        Task OnClientPeerDisconnectedAsync(IClientPeer clientPeer);
        Task OnServerStartedAsync();
        Task OnServerStoppedAsync();
    }
}
