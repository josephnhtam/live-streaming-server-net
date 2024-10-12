using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.PacketDiscarders;
using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders
{
    internal class UpstreamMediaPacketDiscarderFactory : IUpstreamMediaPacketDiscarderFactory
    {
        private readonly RtmpUpstreamConfiguration _config;
        private readonly ILogger<PacketDiscarder> _logger;

        public UpstreamMediaPacketDiscarderFactory(IOptions<RtmpUpstreamConfiguration> config, ILogger<PacketDiscarder> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public IPacketDiscarder Create(string streamPath)
        {
            return new PacketDiscarder(new PacketDiscarderConfiguration
            {
                MaxOutstandingPacketsCount = _config.MaxOutstandingMediaPacketsCount,
                MaxOutstandingPacketsSize = _config.MaxOutstandingMediaPacketsSize,
                TargetOutstandingPacketsCount = _config.TargetOutstandingMediaPacketsCount,
                TargetOutstandingPacketsSize = _config.TargetOutstandingMediaPacketsSize
            },
            (outstandingSize, outstandingCount) => _logger.BeginUpstreamMediaPacketDiscard(streamPath, outstandingSize, outstandingCount),
            (outstandingSize, outstandingCount) => _logger.EndUpstreamMediaPacketDiscard(streamPath, outstandingSize, outstandingCount));
        }
    }
}
