using LiveStreamingServer.Networking.Contracts;
using System.Net;

namespace LiveStreamingServer.Rtmp
{
    public class RtmpServer : IServer
    {
        private readonly IServer _server;

        internal RtmpServer(IServer server)
        {
            _server = server;
        }

        public bool IsStarted => _server.IsStarted;
        public IList<IClientPeerHandle> ClientPeers => _server.ClientPeers;
        public IClientPeerHandle? GetClientPeer(uint clientPeerId) => _server.GetClientPeer(clientPeerId);
        public Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default)
            => _server.RunAsync(localEndpoint, cancellationToken);
    }
}
