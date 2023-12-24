using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerHandler : IClientPeerHandler
    {
        private readonly IServer _server;
        private readonly IFixedNetBuffer _netBuffer;

        private IClientPeerHandle _clientPeer = default!;
        private IRtmpClientPeerContext _peerContext = default!;

        public RtmpClientPeerHandler(IServer server, IClientPeer clientPeer)
        {
            _server = server;
            _clientPeer = clientPeer;

            _netBuffer = FixedNetBuffer.Create(512);
        }

        public void Initialize(IClientPeerHandle clientPeer)
        {
            _clientPeer = clientPeer;
            _peerContext = new RtmpClientPeerContext();
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
