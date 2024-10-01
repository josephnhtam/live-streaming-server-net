using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpStreamFactory : IRtmpStreamFactory
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpCommanderService _commander;
        private readonly ILogger<RtmpStream> _logger;

        public RtmpStreamFactory(
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpCommanderService commander,
            ILogger<RtmpStream> logger)
        {
            _chunkMessageSender = chunkMessageSender;
            _commander = commander;
            _logger = logger;
        }

        public IRtmpStream Create(RtmpClient client, IRtmpStreamContext context)
        {
            return new RtmpStream(client, context, _chunkMessageSender, _commander, _logger);
        }
    }
}
