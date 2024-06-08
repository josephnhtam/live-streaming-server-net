using LiveStreamingServerNet.Networking.Contracts;
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
        private readonly IServer _server;
        private readonly IHlsStorageEventDispatcher _eventDispatcher;
        private readonly IEnumerable<IHlsUploaderCondition> _conditions;
        private readonly IEnumerable<IHlsStorageAdapter> _storageAdapters;
        private readonly ILogger<HlsUploader> _logger;
        private readonly IOptions<HlsUploaderConfiguration> _config;

        public HlsUploaderFactory(
            IServer server,
            IHlsStorageEventDispatcher eventDispatcher,
            IEnumerable<IHlsUploaderCondition> conditions,
            IEnumerable<IHlsStorageAdapter> storageAdapters,
            ILogger<HlsUploader> logger,
            IOptions<HlsUploaderConfiguration> config)
        {
            _server = server;
            _eventDispatcher = eventDispatcher;
            _conditions = conditions;
            _storageAdapters = storageAdapters;
            _logger = logger;
            _config = config;
        }

        public async Task<IHlsUploader?> CreateAsync(StreamProcessingContext context)
        {
            if (!await ShouldUploadAsync(context))
                return null;

            return new HlsUploader(context, _server, _eventDispatcher, _storageAdapters, _logger, _config);
        }

        private async Task<bool> ShouldUploadAsync(StreamProcessingContext context)
        {
            foreach (var condition in _conditions)
            {
                if (!await condition.ShouldUploadAsync(context))
                    return false;
            }

            return true;
        }
    }
}
