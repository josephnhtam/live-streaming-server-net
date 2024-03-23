using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServerEventHandler
    {
        int GetOrder() => 0;
        Task OnListenerCreatedAsync(TcpListener tcpListener);
        Task OnClientAcceptedAsync(TcpClient tcpClient);
        Task OnClientConnectedAsync(IClientHandle client);
        Task OnClientDisconnectedAsync(IClientHandle client);
        Task OnServerStartedAsync();
        Task OnServerStoppedAsync();
    }
}
