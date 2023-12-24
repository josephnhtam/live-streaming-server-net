using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerHandler : IClientPeerHandler
    {
        private readonly IServer _server;
        private readonly IClientPeer _clientPeer;
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpHandshakeHandler _handshakeHandler;
        private readonly IRtmpClientPeerContext _peerContext;

        private readonly IFixedNetBuffer _netBuffer;

        public RtmpClientPeerHandler(
            IServer server,
            IClientPeer clientPeer,
            IRtmpServerContext severContext,
            IRtmpClientPeerContext peerContext,
            IRtmpHandshakeHandler handshakeHandler)
        {
            _server = server;
            _clientPeer = clientPeer;
            _serverContext = severContext;
            _handshakeHandler = handshakeHandler;
            _peerContext = peerContext;

            _netBuffer = FixedNetBuffer.Create(512);
        }

        public async Task<bool> HandleClientPeerLoopAsync(ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await _netBuffer.ReadExactlyAsync(networkStream, 4, cancellationToken);

            int packageSize = _netBuffer.ReadInt32();

            await _netBuffer.ReadExactlyAsync(networkStream, packageSize, cancellationToken);

            string message = _netBuffer.ReadString();
            Console.WriteLine("server: " + message);

            return true;
        }
    }
}
