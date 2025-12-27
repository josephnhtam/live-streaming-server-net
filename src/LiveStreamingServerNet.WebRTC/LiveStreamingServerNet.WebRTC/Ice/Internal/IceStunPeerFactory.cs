using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceIceStunPeerFactory : IIceStunPeerFactory
    {
        private readonly StunPeerConfiguration _config;
        private readonly IDataBufferPool? _bufferPool;

        public IceIceStunPeerFactory(StunPeerConfiguration config, IDataBufferPool? bufferPool)
        {
            _config = config;
            _bufferPool = bufferPool;
        }

        public IStunPeer Create(Socket socket)
        {
            var sender = new SocketStunSender(socket);
            return new StunPeer(sender, _config, _bufferPool);
        }
    }
}
