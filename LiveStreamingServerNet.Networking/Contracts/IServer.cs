using System.Net;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface IServer
    {
        bool IsStarted { get; }
        IList<IClientPeerHandle> ClientPeers { get; }
        IClientPeerHandle? GetClientPeer(uint clientPeerId);
        Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default);
    }
}
