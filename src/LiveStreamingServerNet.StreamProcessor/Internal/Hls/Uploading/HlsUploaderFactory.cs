using LiveStreamingServerNet.StreamProcessor.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading
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

        public IHlsUploader Create(StreamProcessingContext context)
        {
            return new HlsUploader(context, _eventDispatcher, _storageAdapters, _logger, _config);
        }
    }
}
