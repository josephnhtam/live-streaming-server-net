using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding
{
    internal class MediaPacketDiscarder : IMediaPacketDiscarder
    {
        private readonly string _clientId;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger _logger;

        private bool _isDiscarding;

        public MediaPacketDiscarder(string clientId, IOptions<MediaMessageConfiguration> config, ILogger<MediaPacketDiscarder> logger)
        {
            _clientId = clientId;
            _config = config.Value;
            _logger = logger;
        }

        public bool ShouldDiscardMediaPacket(bool isDiscardable, long outstandingSize, long outstandingCount)
        {
            if (!isDiscardable)
            {
                return false;
            }

            if (_isDiscarding)
            {
                if (outstandingSize <= _config.TargetOutstandingMediaMessageSize &&
                    outstandingCount <= _config.TargetOutstandingMediaMessageCount)
                {
                    _logger.ResumeMediaPacket(_clientId, outstandingSize, outstandingCount);
                    _isDiscarding = false;
                    return false;
                }

                return true;
            }

            if (outstandingSize > _config.MaxOutstandingMediaMessageSize ||
                outstandingCount > _config.MaxOutstandingMediaMessageCount)
            {
                _logger.PauseMediaPacket(_clientId, outstandingSize, outstandingCount);
                _isDiscarding = true;
                return true;
            }

            return false;
        }
    }
}
