using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvClientFactory : IFlvClientFactory
    {
        private readonly IFlvWriterFactory _flvWriterFactory;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly ILogger<FlvClient> _logger;

        public FlvClientFactory(
            IFlvWriterFactory flvWriterFactory,
            IFlvMediaTagBroadcasterService mediaTagBroadcaster,
            ILogger<FlvClient> logger)
        {
            _flvWriterFactory = flvWriterFactory;
            _mediaTagBroadcaster = mediaTagBroadcaster;
            _logger = logger;
        }

        public IFlvClient Create(string clientId, string streamPath, IStreamWriter streamWriter, CancellationToken stoppingToken)
        {
            var flvWriter = _flvWriterFactory.Create(streamWriter);
            return new FlvClient(clientId, streamPath, _mediaTagBroadcaster, flvWriter, _logger, stoppingToken);
        }
    }
}
