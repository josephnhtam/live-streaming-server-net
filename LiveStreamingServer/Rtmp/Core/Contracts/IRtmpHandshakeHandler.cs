using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IRtmpHandshakeHandler
    {
        Task<bool> HandleHandshakeAsync(
            IClientPeer clientPeer,
            IRtmpServerContext serverContext,
            IRtmpClientPeerContext peerContext,
            ReadOnlyNetworkStream networkStream,
            IFixedNetBuffer netBuffer,
            CancellationToken cancellationToken);
    }
}
