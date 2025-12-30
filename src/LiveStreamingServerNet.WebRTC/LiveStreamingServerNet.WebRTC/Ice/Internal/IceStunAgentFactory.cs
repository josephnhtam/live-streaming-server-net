using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class IceStunAgentFactory : IIceStunAgentFactory
    {
        private readonly StunAgentConfiguration _config;
        private readonly IDataBufferPool? _bufferPool;

        public IceStunAgentFactory(StunAgentConfiguration config, IDataBufferPool? bufferPool)
        {
            _config = config;
            _bufferPool = bufferPool;
        }

        public IStunAgent Create(Socket socket)
        {
            throw new NotImplementedException();
        }
    }
}
