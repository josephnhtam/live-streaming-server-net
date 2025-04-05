using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal class HlsTransmuxerFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IHlsOutputHandlerFactory _outputHandlerFactory;
        private readonly IHlsPathRegistry _pathRegistry;
        private readonly HlsTransmuxerConfiguration _config;
        private readonly ILogger<HlsTransmuxer> _logger;
        private readonly IBufferPool? _bufferPool;

        public HlsTransmuxerFactory(
            IServiceProvider services,
            IHlsTransmuxerManager transmuxerManager,
            IHlsOutputHandlerFactory outputHandlerFactory,
            IHlsPathRegistry pathRegistry,
            HlsTransmuxerConfiguration config,
            ILogger<HlsTransmuxer> logger,
            IBufferPool? bufferPool)
        {
            _services = services;
            _transmuxerManager = transmuxerManager;
            _outputHandlerFactory = outputHandlerFactory;
            _pathRegistry = pathRegistry;
            _config = config;
            _logger = logger;
            _bufferPool = bufferPool;
        }

        public async Task<IStreamProcessor?> CreateAsync(
            ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                if (!await _config.Condition.IsEnabled(_services, streamPath, streamArguments))
                    return null;

                var manifestOutputPath = await _config.OutputPathResolver.ResolveOutputPath(_services, contextIdentifier, streamPath, streamArguments);
                var tsSegmentOutputPath = GetTsSegmentOutputPath(manifestOutputPath);
                var tsMuxer = new TsMuxer(tsSegmentOutputPath, _bufferPool);

                var config = new HlsTransmuxer.Configuration(
                    contextIdentifier,
                    streamPath,
                    _config.Name,
                    manifestOutputPath,
                    tsSegmentOutputPath,
                    _config.SegmentListSize,
                    _config.DeleteOutdatedSegments,
                    _config.MaxSegmentSize,
                    _config.MaxSegmentBufferSize,
                    _config.MinSegmentLength,
                    _config.AudioOnlySegmentLength,
                    _config.DeleteOutdatedSegments ? _config.CleanupDelay : null
                );

                var outputHandler = _outputHandlerFactory.Create(config);
                return new HlsTransmuxer(client, _transmuxerManager, outputHandler, _pathRegistry, tsMuxer, config, _logger);
            }
            catch
            {
                return null;
            }
        }

        private static string GetTsSegmentOutputPath(string manifestOutputPath)
        {
            var directory = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;
            return Path.Combine(directory, "{seqNum}.ts");
        }
    }
}
