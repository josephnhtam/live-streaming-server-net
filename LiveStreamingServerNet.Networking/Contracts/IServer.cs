using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServer : IServerHandle
    {
        IServiceProvider Services { get; }
        Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default);
    }

    public interface IServerHandle
    {
        bool IsStarted { get; }
        IReadOnlyList<IClientHandle> Clients { get; }
        IClientHandle? GetClient(uint clientId);
    }
}
