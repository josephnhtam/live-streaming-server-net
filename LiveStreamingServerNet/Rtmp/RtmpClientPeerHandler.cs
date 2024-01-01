using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEvents;
using MediatR;

namespace LiveStreamingServerNet.Rtmp
{
    public class RtmpClientPeerHandler : IRtmpClientPeerHandler
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly IMediator _mediator;

        private IRtmpClientPeerContext _peerContext = default!;

        public RtmpClientPeerHandler(IRtmpServerContext serverContext, IMediator mediator)
        {
            _serverContext = serverContext;
            _mediator = mediator;
        }

        public void Initialize(IRtmpClientPeerContext peerContext)
        {
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
                    return await HandleChunkAsync(_peerContext, networkStream, cancellationToken);
            }
        }

        private async Task<bool> HandleHandshakeC0(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC0Event(peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleHandshakeC1(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC1Event(peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleHandshakeC2(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC2Event(peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleChunkAsync(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpChunkEvent(peerContext, networkStream), cancellationToken);
        }

        public void Dispose()
        {
            _serverContext.RemoveClientPeerContext(_peerContext);
        }
    }
}
