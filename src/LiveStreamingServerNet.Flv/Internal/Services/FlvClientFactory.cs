using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
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

        public IFlvClient Create(string clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments, IFlvRequest request, IStreamWriter streamWriter, CancellationToken stoppingToken)
        {
            var flvWriter = _flvWriterFactory.Create(streamWriter);
            return new FlvClient(clientId, streamPath, streamArguments, request, _mediaTagBroadcaster, flvWriter, _logger, stoppingToken);
        }
    }
}
