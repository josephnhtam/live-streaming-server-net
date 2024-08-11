using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class BufferSenderFactory : IBufferSenderFactory
    {
        private readonly IDataBufferPool _dataBufferPool;
        private readonly ILogger<BufferSender> _logger;

        public BufferSenderFactory(IDataBufferPool dataBufferPool, ILogger<BufferSender> logger)
        {
            _dataBufferPool = dataBufferPool;
            _logger = logger;
        }

        public IBufferSender Create()
        {
            return new BufferSender(_dataBufferPool, _logger);
        }
    }
}
