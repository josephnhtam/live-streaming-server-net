using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IServerEventDispatcher
    {
        Task ListenerCreatedAsync(ITcpListener tcpListener);
        Task ClientAcceptedAsync(ITcpClient tcpClient);
        Task ClientConnectedAsync(IClientHandle client);
        Task ClientDisconnectedAsync(IClientHandle client);
        Task ServerStartedAsync();
        Task ServerStoppedAsync();
    }
}
