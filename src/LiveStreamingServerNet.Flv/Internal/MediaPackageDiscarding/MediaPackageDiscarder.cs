using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding
{
    internal class MediaPackageDiscarder : IMediaPackageDiscarder
    {
        private readonly string _clientId;
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger _logger;

        private bool _isDiscarding;

        public MediaPackageDiscarder(string clientId, IOptions<MediaMessageConfiguration> config, ILogger<MediaPackageDiscarder> logger)
        {
            _clientId = clientId;
            _config = config.Value;
            _logger = logger;
        }

        public bool ShouldDiscardMediaPackage(bool isDiscardable, long outstandingSize, long outstandingCount)
        {
            if (!isDiscardable)
            {
                _isDiscarding = false;
                return false;
            }

            if (_isDiscarding)
            {
                if (outstandingSize <= _config.TargetOutstandingMediaMessageSize &&
                    outstandingCount <= _config.TargetOutstandingMediaMessageCount)
                {
                    _logger.ResumeMediaPackage(_clientId, outstandingSize, outstandingCount);
                    _isDiscarding = false;
                    return false;
                }

                return true;
            }

            if (outstandingSize > _config.MaxOutstandingMediaMessageSize ||
                outstandingCount > _config.MaxOutstandingMediaMessageCount)
            {
                _logger.PauseMediaPackage(_clientId, outstandingSize, outstandingCount);
                _isDiscarding = true;
                return true;
            }

            return false;
        }
    }
}
