using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8.Contracts;
using Microsoft.Extensions.Logging;
using static LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.HlsTransmuxer;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal class HlsOutputHandlerFactory : IHlsOutputHandlerFactory
    {
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly IManifestWriter _manifestWriter;
        private readonly ILogger<HlsOutputHandler> _logger;

        public HlsOutputHandlerFactory(
            IHlsCleanupManager cleanupManager,
            IManifestWriter manifestWriter,
            ILogger<HlsOutputHandler> logger)
        {
            _cleanupManager = cleanupManager;
            _manifestWriter = manifestWriter;
            _logger = logger;
        }

        public IHlsOutputHandler Create(Configuration config)
        {
            return new HlsOutputHandler(_manifestWriter, _cleanupManager, config, _logger);
        }
    }
}
