using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvWriterFactory : IFlvWriterFactory
    {
        private readonly ILogger<FlvWriter> _logger;

        public FlvWriterFactory(ILogger<FlvWriter> logger)
        {
            _logger = logger;
        }

        public IFlvWriter Create(IFlvClient client, IStreamWriter streamWriter)
        {
            return new FlvWriter(client, streamWriter, _logger);
        }
    }
}
