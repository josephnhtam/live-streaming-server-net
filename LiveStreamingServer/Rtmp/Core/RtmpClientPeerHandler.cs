using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerHandler : IRtmpClientPeerHandler
    {
        private readonly IServer _server;
        private readonly IMediator _mediator;

        private IClientPeerHandle _clientPeer = default!;
        private IRtmpClientPeerContext _peerContext = default!;

        public RtmpClientPeerHandler(IServer server, IMediator mediator)
        {
            _server = server;
            _mediator = mediator;
        }

        public void Initialize(IClientPeerHandle clientPeer, IRtmpClientPeerContext peerContext)
        {
            _clientPeer = clientPeer;
            _peerContext = peerContext;
            _peerContext.State = RtmpClientPeerState.HandshakeC0;
        }

        public async Task<bool> HandleClientPeerLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            switch (_peerContext.State)
            {
                case RtmpClientPeerState.HandshakeC0:
                    return await HandleHandshakeC0(_peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC1:
                    return await HandleHandshakeC1(_peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC2:
                    return await HandleHandshakeC2(_peerContext, networkStream, cancellationToken);
                default:
                    return await HandleChunkAsync(_server, _peerContext, networkStream, cancellationToken);
            }
        }

        private async Task<bool> HandleHandshakeC0(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC0Event(_clientPeer, peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleHandshakeC1(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC1Event(_clientPeer, peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleHandshakeC2(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC2Event(_clientPeer, peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleChunkAsync(IServer server, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpChunkEvent(server, _clientPeer, peerContext, networkStream), cancellationToken);
        }
    }
}
