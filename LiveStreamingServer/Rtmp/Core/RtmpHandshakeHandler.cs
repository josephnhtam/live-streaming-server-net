using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpHandshakeHandler : IRtmpHandshakeHandler
    {
        public Task<bool> HandleHandshakeAsync(
            IClientPeer clientPeer,
            IRtmpServerContext serverContext,
            IRtmpClientPeerContext peerContext,
            ReadOnlyNetworkStream networkStream,
            IFixedNetBuffer netBuffer,
            CancellationToken cancellationToken)
        {
            switch (peerContext.State)
            {
                case RtmpClientPeerState.BeforeHandshake:
                    return HandleBeforeHandshakeAsync(clientPeer, serverContext, peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC0Received:
                    return HandleC0ReceivedAsync(clientPeer, serverContext, peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC1Received:
                    return HandleC1ReceivedAsync(clientPeer, serverContext, peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC2Received:
                    return HandleC2ReceivedAsync(clientPeer, serverContext, peerContext, networkStream, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Task<bool> HandleC2ReceivedAsync(IClientPeer clientPeer, IRtmpServerContext serverContext, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<bool> HandleC1ReceivedAsync(IClientPeer clientPeer, IRtmpServerContext serverContext, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<bool> HandleC0ReceivedAsync(IClientPeer clientPeer, IRtmpServerContext serverContext, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<bool> HandleBeforeHandshakeAsync(IClientPeer clientPeer, IRtmpServerContext serverContext, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
