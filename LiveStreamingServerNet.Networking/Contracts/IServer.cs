using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServer
    {
        bool IsStarted { get; }
        IList<IClientHandle> Clients { get; }
        IClientHandle? GetClient(uint clientClientId);
        Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default);
    }
}
