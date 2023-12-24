using System.Net;

namespace LiveStreamingServer.Networking.Contracts
{
    public interface IServer
    {
        bool IsStarted { get; }
        IList<IClientPeer> ClientPeers { get; }
        IClientPeer? GetClientPeer(uint clientPeerId);
        Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default);
    }
}
