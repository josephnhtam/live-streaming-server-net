using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpClientSessionContextFactory : IRtmpClientSessionContextFactory
    {
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpClientSessionContext> _logger;
        private readonly IBufferPool? _bufferPool;

        public RtmpClientSessionContextFactory(IOptions<RtmpServerConfiguration> config, ILogger<RtmpClientSessionContext> logger, IBufferPool? bufferPool = null)
        {
            _config = config.Value;
            _logger = logger;
            _bufferPool = bufferPool;
        }

        public IRtmpClientSessionContext Create(ISessionHandle client)
        {
            return new RtmpClientSessionContext(client, _bufferPool, _config, _logger);
        }
    }
}
