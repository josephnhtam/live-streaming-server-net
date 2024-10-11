using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding
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

        public IMediaPacketDiscarder Create(string clientId)
        {
            return new MediaPacketDiscarder(clientId, _config, _logger);
        }
    }
}
