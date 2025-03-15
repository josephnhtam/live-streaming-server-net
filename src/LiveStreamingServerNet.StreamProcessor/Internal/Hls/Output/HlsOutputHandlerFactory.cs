using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using static LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.HlsTransmuxer;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal class HlsOutputHandlerFactory : IHlsOutputHandlerFactory
    {
        private readonly IDataBufferPool _bufferPool;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly IMediaManifestWriter _manifestWriter;
        private readonly ILogger<HlsOutputHandler> _logger;

        public HlsOutputHandlerFactory(
            IDataBufferPool bufferPool,
            IHlsCleanupManager cleanupManager,
            IMediaManifestWriter manifestWriter,
            ILogger<HlsOutputHandler> logger)
        {
            _bufferPool = bufferPool;
            _cleanupManager = cleanupManager;
            _manifestWriter = manifestWriter;
            _logger = logger;
        }

        public IHlsOutputHandler Create(Configuration config)
        {
            return new HlsOutputHandler(_bufferPool, _manifestWriter, _cleanupManager, config, _logger);
        }
    }
}
