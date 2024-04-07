using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServerEventHandler
    {
        int GetOrder() => 0;
        Task OnListenerCreatedAsync(IEventContext context, ITcpListener tcpListener);
        Task OnClientAcceptedAsync(IEventContext context, ITcpClient tcpClient);
        Task OnClientConnectedAsync(IEventContext context, IClientHandle client);
        Task OnClientDisconnectedAsync(IEventContext context, IClientHandle client);
        Task OnServerStartedAsync(IEventContext context);
        Task OnServerStoppedAsync(IEventContext context);
    }
}
