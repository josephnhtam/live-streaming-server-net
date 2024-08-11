using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Contracts
{
    public interface IServerEventHandler
    {
        int GetOrder() => 0;
        Task OnListenerCreatedAsync(IEventContext context, ITcpListener tcpListener);
        Task OnClientAcceptedAsync(IEventContext context, ITcpClient tcpClient);
        Task OnClientConnectedAsync(IEventContext context, ISessionHandle client);
        Task OnClientDisconnectedAsync(IEventContext context, ISessionInfo client);
        Task OnServerStartedAsync(IEventContext context);
        Task OnServerStoppedAsync(IEventContext context);
    }
}
