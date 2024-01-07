using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    internal interface IServerEventDispatcher
    {
        Task ListenerCreatedAsync(TcpListener tcpListener);
        Task ClientAcceptedAsync(TcpClient tcpClient);
        Task ClientConnectedAsync(IClientHandle client);
        Task ClientDisconnectedAsync(IClientHandle client);
        Task ServerStartedAsync();
        Task ServerStoppedAsync();
    }
}
