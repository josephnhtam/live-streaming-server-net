using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal class HlsTransmuxerFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly HlsTransmuxerConfiguration _config;

        private readonly IBufferPool? _bufferPool;

        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IHlsPathRegistry _pathRegistry;
        private readonly ILogger<HlsTransmuxer> _transmuxerLogger;

        private readonly IDataBufferPool _dataBufferPool;
        private readonly IMediaManifestWriter _mediaManifestWriter;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly ILogger<HlsOutputHandler> _outputHandlerLogger;

        public HlsTransmuxerFactory(
            IServiceProvider services,
            HlsTransmuxerConfiguration config)
        {
            _services = services;
            _config = config;

            _bufferPool = _services.GetService<IBufferPool>();

            _transmuxerManager = _services.GetRequiredService<IHlsTransmuxerManager>();
            _pathRegistry = _services.GetRequiredService<IHlsPathRegistry>();
            _transmuxerLogger = _services.GetRequiredService<ILogger<HlsTransmuxer>>();

            _dataBufferPool = _services.GetRequiredService<IDataBufferPool>();
            _mediaManifestWriter = _services.GetRequiredService<IMediaManifestWriter>();
            _cleanupManager = _services.GetRequiredService<IHlsCleanupManager>();
            _outputHandlerLogger = _services.GetRequiredService<ILogger<HlsOutputHandler>>();
        }

        public async Task<IStreamProcessor?> CreateAsync(
            ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!await _config.Condition.IsEnabled(_services, streamPath, streamArguments).ConfigureAwait(false))
                return null;

            var manifestOutputPath = await _config.OutputPathResolver.ResolveOutputPath(_services, contextIdentifier, streamPath, streamArguments).ConfigureAwait(false);
            var tsSegmentOutputPath = GetTsSegmentOutputPath(manifestOutputPath);

            var outputHandlerConfig = new HlsOutputHandler.Configuration(
                contextIdentifier,
                streamPath,
                _config.Name,
                manifestOutputPath,
                _config.SegmentListSize,
                _config.DeleteOutdatedSegments,
                _config.DeleteOutdatedSegments ? _config.CleanupDelay : null
            );

            var transmuxerConfig = new HlsTransmuxer.Configuration(
                contextIdentifier,
                streamPath,
                _config.Name,
                manifestOutputPath,
                tsSegmentOutputPath,
                _config.MaxSegmentSize,
                _config.MaxSegmentBufferSize,
                _config.MinSegmentLength,
                _config.AudioOnlySegmentLength
            );

            var tsMuxer = new TsMuxer(tsSegmentOutputPath, _bufferPool);

            var outputHandler = new HlsOutputHandler(
                bufferPool: _dataBufferPool,
                manifestWriter: _mediaManifestWriter,
                cleanupManager: _cleanupManager,
                config: outputHandlerConfig,
                logger: _outputHandlerLogger);

            return new HlsTransmuxer(
                client: client,
                transmuxerManager: _transmuxerManager,
                pathRegistry: _pathRegistry,
                tsMuxer: tsMuxer,
                outputHandler: outputHandler,
                mediaPacketInterceptor: null,
                config: transmuxerConfig,
                logger: _transmuxerLogger);
        }

        private static string GetTsSegmentOutputPath(string manifestOutputPath)
        {
            var directory = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;
            return Path.Combine(directory, "{seqNum}.ts");
        }
    }
}
