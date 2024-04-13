using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding
{
    internal class MediaPackageDiscarderFactory : IMediaPackageDiscarderFactory
    {
        private readonly IOptions<MediaMessageConfiguration> _config;
        private readonly ILogger<MediaPackageDiscarder> _logger;

        public MediaPackageDiscarderFactory(IOptions<MediaMessageConfiguration> config, ILogger<MediaPackageDiscarder> logger)
        {
            _config = config;
            _logger = logger;
        }

        public IMediaPackageDiscarder Create(string clientId)
        {
            return new MediaPackageDiscarder(clientId, _config, _logger);
        }
    }
}
