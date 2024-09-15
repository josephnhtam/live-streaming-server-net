using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Client.Contracts
{
    public interface IClientEventHandler
    {
        int GetOrder() => 0;
        Task OnClientConnectedAsync(IEventContext context, ISessionHandle session);
        Task OnClientStoppedAsync(IEventContext context);
    }
}
