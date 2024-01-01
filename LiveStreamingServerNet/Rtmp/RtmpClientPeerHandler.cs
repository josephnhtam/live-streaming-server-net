using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEvents;
using MediatR;

namespace LiveStreamingServerNet.Rtmp
{
    public class RtmpClientPeerHandler : IRtmpClientPeerHandler
    {
        private readonly IMediator _mediator;
        private readonly IEnumerable<IRtmpServerEventHandler> _serverEventHandlers;

        private IRtmpClientPeerContext _peerContext = default!;

        public RtmpClientPeerHandler(IMediator mediator, IEnumerable<IRtmpServerEventHandler> serverEventHandlers)
        {
            _mediator = mediator;
            _serverEventHandlers = serverEventHandlers;
        }

        public void Initialize(IRtmpClientPeerContext peerContext)
        {
            _peerContext = peerContext;
            _peerContext.State = RtmpClientPeerState.HandshakeC0;

            OnRtmpClientCreated();
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
            OnRtmpClientDisposed();
        }

        private void OnRtmpClientCreated()
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                serverEventHandler.OnRtmpClientCreated(_peerContext);
        }

        private void OnRtmpClientDisposed()
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                serverEventHandler.OnRtmpClientDisposed(_peerContext);
        }
    }
}
