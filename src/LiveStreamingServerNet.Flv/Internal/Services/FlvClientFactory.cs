using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvClientFactory : IFlvClientFactory
    {
        private readonly IFlvWriterFactory _flvWriterFactory;
        private readonly IFlvMediaTagManagerService _mediaTagManager;

        public FlvClientFactory(IFlvWriterFactory flvWriterFactory, IFlvMediaTagManagerService mediaTagManager)
        {
            _flvWriterFactory = flvWriterFactory;
            _mediaTagManager = mediaTagManager;
        }

        public IFlvClient Create(string clientId, string streamPath, IStreamWriter streamWriter, CancellationToken stoppingToken)
        {
            var flvWriter = _flvWriterFactory.Create();
            return new FlvClient(_mediaTagManager, flvWriter, clientId, streamPath, streamWriter, stoppingToken);
        }
    }
}
