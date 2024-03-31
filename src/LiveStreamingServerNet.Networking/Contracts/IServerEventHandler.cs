using LiveStreamingServerNet.Utilities.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServerEventHandler
    {
        int GetOrder() => 0;
        Task OnListenerCreatedAsync(IEventContext context, TcpListener tcpListener);
        Task OnClientAcceptedAsync(IEventContext context, TcpClient tcpClient);
        Task OnClientConnectedAsync(IEventContext context, IClientHandle client);
        Task OnClientDisconnectedAsync(IEventContext context, IClientHandle client);
        Task OnServerStartedAsync(IEventContext context);
        Task OnServerStoppedAsync(IEventContext context);
    }
}
