using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;
using MediatR;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerHandler : IRtmpClientPeerHandler
    {
        private readonly IServer _server;
        private readonly IMediator _mediator;
        private readonly INetBufferPool _netBufferPool;

        private IClientPeerHandle _clientPeer = default!;
        private IRtmpClientPeerContext _peerContext = default!;

        public RtmpClientPeerHandler(IServer server, IMediator mediator, INetBufferPool netBufferPool)
        {
            _server = server;
            _mediator = mediator;
            _netBufferPool = netBufferPool;
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
            return await _mediator.Send(new RtmpHandshakeC0Request(_clientPeer, peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleHandshakeC1(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC1Request(_clientPeer, peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleHandshakeC2(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new RtmpHandshakeC2Request(_clientPeer, peerContext, networkStream), cancellationToken);
        }

        private async Task<bool> HandleChunkAsync(IServer server, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            Console.WriteLine("server: HandleChunkAsync");
            var netBuffer = _netBufferPool.ObtainNetBuffer();

            try
            {
                await netBuffer.ReadFromAsync(networkStream, 128, cancellationToken);

                Console.WriteLine("server: 128 bytes received");

                return true;
            }
            finally
            {
                _netBufferPool.RecycleNetBuffer(netBuffer);
            }
        }
    }
}
