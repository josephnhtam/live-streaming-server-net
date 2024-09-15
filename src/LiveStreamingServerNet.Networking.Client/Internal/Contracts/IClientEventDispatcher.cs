using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Client.Internal.Contracts
{
    internal interface IClientEventDispatcher
    {
        Task ClientConnectedAsync(ISessionHandle client);
        Task ClientStoppedAsync();
    }
}
