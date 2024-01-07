using LiveStreamingServerNet.Networking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking.Contracts
{
    public interface IServerEventHandler
    {
        Task OnListenerCreatedAsync(TcpListener tcpListener);
        Task OnClientAcceptedAsync(TcpClient tcpClient);
        Task OnClientConnectedAsync(IClientHandle client);
        Task OnClientDisconnectedAsync(IClientHandle client);
        Task OnServerStartedAsync();
        Task OnServerStoppedAsync();
    }
}
