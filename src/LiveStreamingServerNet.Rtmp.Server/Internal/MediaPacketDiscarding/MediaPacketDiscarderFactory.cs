using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPacketDiscarding.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.MediaPacketDiscarding
{
    internal class MediaPacketDiscarderFactory : IMediaPacketDiscarderFactory
    {
        private readonly IOptions<MediaMessageConfiguration> _config;
        private readonly ILogger<MediaPacketDiscarder> _logger;

        public MediaPacketDiscarderFactory(IOptions<MediaMessageConfiguration> config, ILogger<MediaPacketDiscarder> logger)
        {
            _config = config;
            _logger = logger;
        }

        public IMediaPacketDiscarder Create(uint clientId)
        {
            return new MediaPacketDiscarder(clientId, _config, _logger);
        }
    }
}
