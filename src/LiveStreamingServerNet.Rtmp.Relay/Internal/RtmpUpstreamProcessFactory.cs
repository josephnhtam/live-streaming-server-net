using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal
{
    internal class RtmpUpstreamProcessFactory : IRtmpUpstreamProcessFactory
    {
        private readonly IRtmpOriginResolver _originResolver;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IBufferPool _bufferPool;
        private readonly IDataBufferPool _dataBufferPool;
        private readonly IUpstreamMediaPacketDiscarderFactory _packetDiscarderFactory;
        private readonly IOptions<RtmpUpstreamConfiguration> _config;
        private readonly ILogger<RtmpUpstreamProcess> _logger;

        public RtmpUpstreamProcessFactory(
            IRtmpOriginResolver originResolver,
            IRtmpStreamManagerService streamManager,
            IBufferPool bufferPool,
            IDataBufferPool dataBufferPool,
            IUpstreamMediaPacketDiscarderFactory packetDiscarderFactory,
            IOptions<RtmpUpstreamConfiguration> config,
            ILogger<RtmpUpstreamProcess> logger)
        {
            _originResolver = originResolver;
            _streamManager = streamManager;
            _bufferPool = bufferPool;
            _dataBufferPool = dataBufferPool;
            _packetDiscarderFactory = packetDiscarderFactory;
            _config = config;
            _logger = logger;
        }

        public IRtmpUpstreamProcess Create(IRtmpPublishStreamContext publishStreamContext)
        {
            return new RtmpUpstreamProcess(
                publishStreamContext,
                _originResolver,
                _streamManager,
                _bufferPool,
                _dataBufferPool,
                _packetDiscarderFactory,
                _config,
                _logger);
        }
    }
}
