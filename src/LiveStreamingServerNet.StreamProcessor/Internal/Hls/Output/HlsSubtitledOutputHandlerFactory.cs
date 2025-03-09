using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using static LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.HlsTransmuxer;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal class HlsSubtitledOutputHandlerFactory : IHlsOutputHandlerFactory
    {
        private readonly IDataBufferPool _bufferPool;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly IMediaManifestWriter _manifestWriter;
        private readonly IReadOnlyList<SubtitleTranscriptionStreamFactory> _subtitleStreamFactories;
        private readonly ILogger<HlsSubtitledOutputHandler> _logger;

        public HlsSubtitledOutputHandlerFactory(
            IDataBufferPool bufferPool,
            IHlsCleanupManager cleanupManager,
            IMediaManifestWriter manifestWriter,
            IReadOnlyList<SubtitleTranscriptionStreamFactory> subtitleStreamFactories,
            ILogger<HlsSubtitledOutputHandler> logger)
        {
            _bufferPool = bufferPool;
            _cleanupManager = cleanupManager;
            _manifestWriter = manifestWriter;
            _subtitleStreamFactories = subtitleStreamFactories;
            _logger = logger;
        }

        public IHlsOutputHandler Create(Configuration config)
        {
            return new HlsSubtitledOutputHandler(_bufferPool, _manifestWriter, _cleanupManager, _subtitleStreamFactories, config, _logger);
        }
    }
}
