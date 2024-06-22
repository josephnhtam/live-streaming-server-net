using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class ClientBufferSenderFactory : IClientBufferSenderFactory
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly ILogger<ClientBufferSender> _logger;

        public ClientBufferSenderFactory(IDataBufferPool dataBufferPool, ILogger<ClientBufferSender> logger)
        {
            _dataBufferPool = dataBufferPool;
            _logger = logger;
        }

        public IClientBufferSender Create(uint clientId)
        {
            return new ClientBufferSender(clientId, _dataBufferPool, _logger);
        }
    }
}
