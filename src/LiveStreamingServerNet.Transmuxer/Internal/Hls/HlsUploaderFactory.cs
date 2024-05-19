using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal class HlsUploaderFactory : IHlsUploaderFactory
    {
        private readonly IHlsStorageEventDispatcher _eventDispatcher;
        private readonly IEnumerable<IHlsStorageAdapter> _storageAdapters;
        private readonly ILogger<HlsUploader> _logger;
        private readonly IOptions<HlsUploaderConfiguration> _config;

        public HlsUploaderFactory(
            IHlsStorageEventDispatcher eventDispatcher,
            IEnumerable<IHlsStorageAdapter> storageAdapters,
            ILogger<HlsUploader> logger,
            IOptions<HlsUploaderConfiguration> config)
        {
            _eventDispatcher = eventDispatcher;
            _storageAdapters = storageAdapters;
            _logger = logger;
            _config = config;
        }

        public IHlsUploader Create(TransmuxingContext context)
        {
            return new HlsUploader(context, _eventDispatcher, _storageAdapters, _logger, _config);
        }
    }
}
