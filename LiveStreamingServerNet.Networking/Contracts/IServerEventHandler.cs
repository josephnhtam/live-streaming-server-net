using LiveStreamingServerNet.Networking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IServerEventHandler
    {
        Task OnListenerCreatedAsync(TcpListener tcpListener);
        Task OnClientAcceptedAsync(TcpClient tcpClient);
        Task OnClientConnectedAsync(IClient clientClient);
        Task OnClientDisconnectedAsync(IClient clientClient);
        Task OnServerStartedAsync();
        Task OnServerStoppedAsync();
    }
}
