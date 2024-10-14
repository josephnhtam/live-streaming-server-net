using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Utilities.PacketDiscarders;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarders
{
    internal class MediaPacketDiscarderFactory : IMediaPacketDiscarderFactory
    {
        private readonly MediaStreamingConfiguration _config;
        private readonly ILogger<PacketDiscarder> _logger;

        public MediaPacketDiscarderFactory(IOptions<MediaStreamingConfiguration> config, ILogger<PacketDiscarder> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public IPacketDiscarder Create(string clientId)
        {
            return new PacketDiscarder(new PacketDiscarderConfiguration
            {
                MaxOutstandingPacketsCount = _config.MaxOutstandingMediaPacketsCount,
                MaxOutstandingPacketsSize = _config.MaxOutstandingMediaPacketsSize,
                TargetOutstandingPacketsCount = _config.TargetOutstandingMediaPacketsCount,
                TargetOutstandingPacketsSize = _config.TargetOutstandingMediaPacketsSize
            },
            (outstandingSize, outstandingCount) => _logger.BeginMediaPacketDiscard(clientId, outstandingSize, outstandingCount),
            (outstandingSize, outstandingCount) => _logger.EndMediaPacketDiscard(clientId, outstandingSize, outstandingCount));
        }
    }
}
