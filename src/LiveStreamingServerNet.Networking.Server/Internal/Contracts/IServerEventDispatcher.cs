using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Server.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Internal.Contracts
{
    internal interface IServerEventDispatcher
    {
        Task ListenerCreatedAsync(ITcpListener tcpListener);
        Task ClientAcceptedAsync(ITcpClient tcpClient);
        Task ClientConnectedAsync(ISessionHandle client);
        Task ClientDisconnectedAsync(ISessionInfo client);
        Task ServerStartedAsync();
        Task ServerStoppedAsync();
    }
}
