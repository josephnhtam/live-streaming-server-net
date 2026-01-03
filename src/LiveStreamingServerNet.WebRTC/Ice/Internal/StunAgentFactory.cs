using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Stun.Configurations;
using LiveStreamingServerNet.WebRTC.Stun.Internal;
using LiveStreamingServerNet.WebRTC.Stun.Internal.Contracts;
using LiveStreamingServerNet.WebRTC.Udp.Internal.Contracts;

namespace LiveStreamingServerNet.WebRTC.Ice.Internal
{
    internal class StunAgentFactory : IStunAgentFactory
    {
        private readonly StunAgentConfiguration _config;
        private readonly IDataBufferPool? _bufferPool;

        public StunAgentFactory(StunAgentConfiguration config, IDataBufferPool? bufferPool = null)
        {
            _config = config;
            _bufferPool = bufferPool;
        }

        public IStunAgent Create(IUdpTransport transport)
        {
            var stunSender = new UdpStunSender(transport);
            return new StunAgent(stunSender, _config, _bufferPool);
        }
    }
}
