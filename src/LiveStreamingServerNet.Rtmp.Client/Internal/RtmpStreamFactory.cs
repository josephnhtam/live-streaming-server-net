using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpStreamFactory : IRtmpStreamFactory
    {
        private readonly IRtmpCommanderService _commander;
        private readonly IRtmpMediaDataSenderService _mediaDataSender;
        private readonly ILogger<RtmpStream> _logger;

        public RtmpStreamFactory(
            IRtmpCommanderService commander,
            IRtmpMediaDataSenderService mediaDataSender,
            ILogger<RtmpStream> logger)
        {
            _commander = commander;
            _mediaDataSender = mediaDataSender;
            _logger = logger;
        }

        public IRtmpStream Create(IRtmpStreamContext context)
        {
            return new RtmpStream(context, _commander, _mediaDataSender, _logger);
        }
    }
}
