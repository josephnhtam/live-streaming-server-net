using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class NetBufferSenderFactory : INetBufferSenderFactory
    {
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger<NetBufferSender> _logger;

        public NetBufferSenderFactory(INetBufferPool netBufferPool, ILogger<NetBufferSender> logger)
        {
            _netBufferPool = netBufferPool;
            _logger = logger;
        }

        public INetBufferSender Create(uint clientId)
        {
            return new NetBufferSender(clientId, _netBufferPool, _logger);
        }
    }
}
