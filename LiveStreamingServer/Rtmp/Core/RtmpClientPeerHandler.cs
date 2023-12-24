using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerHandler : IRtmpClientPeerHandler
    {
        private readonly IServer _server;
        private readonly INetBufferPool _netBufferPool;

        private IClientPeerHandle _clientPeer = default!;
        private IRtmpClientPeerContext _peerContext = default!;

        public RtmpClientPeerHandler(IServer server, INetBufferPool netBufferPool)
        {
            _server = server;
            _netBufferPool = netBufferPool;
        }

        public void Initialize(IClientPeerHandle clientPeer, IRtmpClientPeerContext peerContext)
        {
            _clientPeer = clientPeer;
            _peerContext = peerContext;
            _peerContext.State = RtmpClientPeerState.HandshakeDone;
        }

        public async Task<bool> HandleClientPeerLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            switch (_peerContext.State)
            {
                case RtmpClientPeerState.BeforeHandshake:
                    return await HandleBeforeHandshakeAsync(_peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC0Received:
                    return await HandleC0ReceivedAsync(_peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC1Received:
                    return await HandleC1ReceivedAsync(_peerContext, networkStream, cancellationToken);
                case RtmpClientPeerState.HandshakeC2Received:
                    return await HandleC2ReceivedAsync(_peerContext, networkStream, cancellationToken);
                default:
                    return await HandleChunkAsync(_server, _peerContext, networkStream, cancellationToken);
            }
        }

        private async Task<bool> HandleChunkAsync(IServer server, IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            var netBuffer = _netBufferPool.ObtainNetBuffer();

            try
            {
                await netBuffer.ReadFromAsync(networkStream, 4, cancellationToken);

                int packageSize = netBuffer.ReadInt32();

                await netBuffer.ReadFromAsync(networkStream, packageSize, cancellationToken);

                string message = netBuffer.ReadString();
                Console.WriteLine("server: " + message);

                return true;
            }
            finally
            {
                _netBufferPool.RecycleNetBuffer(netBuffer);
            }
        }

        private Task<bool> HandleC2ReceivedAsync(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<bool> HandleC1ReceivedAsync(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<bool> HandleC0ReceivedAsync(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<bool> HandleBeforeHandshakeAsync(IRtmpClientPeerContext peerContext, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
