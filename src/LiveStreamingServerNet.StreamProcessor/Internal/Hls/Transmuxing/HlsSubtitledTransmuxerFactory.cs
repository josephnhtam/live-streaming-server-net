using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal class HlsSubtitledTransmuxerFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly IReadOnlyList<SubtitleTranscriptionConfiguration> _subtitleTranscriptionConfigs;
        private readonly HlsTransmuxerConfiguration _config;

        private readonly IBufferPool? _bufferPool;

        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IHlsPathRegistry _pathRegistry;
        private readonly ILogger<HlsTransmuxer> _transmuxerLogger;

        private readonly ISubtitleTranscriberFactory _subtitleTranscriberFactory;

        private readonly IDataBufferPool _dataBufferPool;
        private readonly IMasterManifestWriter _masterManifestWriter;
        private readonly IMediaManifestWriter _mediaManifestWriter;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly ILogger<HlsSubtitledOutputHandler> _outputHandlerLogger;

        public HlsSubtitledTransmuxerFactory(
            IServiceProvider services,
            IReadOnlyList<SubtitleTranscriptionConfiguration> subtitleTranscriptionConfigs,
            HlsTransmuxerConfiguration config)
        {
            _services = services;
            _subtitleTranscriptionConfigs = subtitleTranscriptionConfigs;
            _config = config;

            _bufferPool = _services.GetService<IBufferPool>();

            _transmuxerManager = _services.GetRequiredService<IHlsTransmuxerManager>();
            _pathRegistry = _services.GetRequiredService<IHlsPathRegistry>();
            _transmuxerLogger = _services.GetRequiredService<ILogger<HlsTransmuxer>>();

            _subtitleTranscriberFactory = _services.GetRequiredService<ISubtitleTranscriberFactory>();

            _dataBufferPool = _services.GetRequiredService<IDataBufferPool>();
            _masterManifestWriter = _services.GetRequiredService<IMasterManifestWriter>();
            _mediaManifestWriter = _services.GetRequiredService<IMediaManifestWriter>();
            _cleanupManager = _services.GetRequiredService<IHlsCleanupManager>();
            _outputHandlerLogger = _services.GetRequiredService<ILogger<HlsSubtitledOutputHandler>>();
        }

        public async Task<IStreamProcessor?> CreateAsync(
            ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                if (!await _config.Condition.IsEnabled(_services, streamPath, streamArguments))
                    return null;

                var initialProgramDateTime = DateTime.UtcNow;
                var masterManifestOutputPath = await _config.OutputPathResolver.ResolveOutputPath(_services, contextIdentifier, streamPath, streamArguments);
                string mediaManifestOutputPath = GetMediaManifestOutputPath(masterManifestOutputPath);
                var tsSegmentOutputPath = GetTsSegmentOutputPath(masterManifestOutputPath);

                var outputHandlerConfig = new HlsSubtitledOutputHandler.Configuration(
                    contextIdentifier,
                    streamPath,
                    _config.Name,
                    masterManifestOutputPath,
                    mediaManifestOutputPath,
                    _config.SegmentListSize,
                    _config.DeleteOutdatedSegments,
                    _config.DeleteOutdatedSegments ? _config.CleanupDelay : null
                );

                var transmuxerConfig = new HlsTransmuxer.Configuration(
                    contextIdentifier,
                    streamPath,
                    _config.Name,
                    masterManifestOutputPath,
                    tsSegmentOutputPath,
                    _config.MaxSegmentSize,
                    _config.MaxSegmentBufferSize,
                    _config.MinSegmentLength,
                    _config.AudioOnlySegmentLength
                );

                var tsMuxer = new TsMuxer(tsSegmentOutputPath, _bufferPool);

                var subtitleTranscribers = CreateSubtitleTranscribers(
                    contextIdentifier,
                    streamPath,
                    masterManifestOutputPath,
                    _dataBufferPool,
                    _subtitleTranscriberFactory,
                    _subtitleTranscriptionConfigs,
                    initialProgramDateTime);

                var meidaPacketInterceptor = new HlsSubtitledMediaPacketInterceptor(subtitleTranscribers);

                var outputHandler = new HlsSubtitledOutputHandler(
                    bufferPool: _dataBufferPool,
                    masterManifestWriter: _masterManifestWriter,
                    mediaManifestWriter: _mediaManifestWriter,
                    cleanupManager: _cleanupManager,
                    subtitleTranscribers: subtitleTranscribers,
                    initialProgramDateTime: initialProgramDateTime,
                    config: outputHandlerConfig,
                    logger: _outputHandlerLogger);

                return new HlsTransmuxer(
                    client: client,
                    transmuxerManager: _transmuxerManager,
                    pathRegistry: _pathRegistry,
                    tsMuxer: tsMuxer,
                    outputHandler: outputHandler,
                    mediaPacketInterceptor: meidaPacketInterceptor,
                    config: transmuxerConfig,
                    logger: _transmuxerLogger);
            }
            catch
            {
                return null;
            }
        }

        private List<ISubtitleTranscriber> CreateSubtitleTranscribers(
            Guid contextIdentifier,
            string streamPath,
            string masterManifestOutputPath,
            IDataBufferPool bufferPool,
            ISubtitleTranscriberFactory subtitleTranscriberFactory,
            IReadOnlyList<SubtitleTranscriptionConfiguration> subtitleStreamFactories,
            DateTime initialProgramDateTime)
        {
            var inputStreamWriterFactory = new FlvAudioStreamWriterFactory(bufferPool);

            return subtitleStreamFactories.Select((x, idx) =>
            {
                var rootDir = Path.GetDirectoryName(masterManifestOutputPath) ?? string.Empty;
                var subtitleManifestOutputPath = Path.Combine(rootDir, $"subtitle_{idx}.m3u8");
                var subtitleSegmentOutputPath = Path.Combine(rootDir, $$"""subtitle_{{idx}}_{seqNum}.vtt""");

                var config = new SubtitleTranscriberConfiguration(
                    contextIdentifier,
                    streamPath,
                    _config.Name,
                    subtitleManifestOutputPath,
                    subtitleSegmentOutputPath,
                    _config.DeleteOutdatedSegments
                );

                var transcriptionStreamFactory = x.TranscriptionStreamFactory.Invoke(_services);

                var subtitleCueExtractorFactory = x.SubtitleCueExtractorFactory?.Invoke(_services) ??
                    new SubtitleCueExtractorFactory();

                return subtitleTranscriberFactory.Create(
                    options: x.Options,
                    config: config,
                    transcriptionStream: transcriptionStreamFactory.Create(inputStreamWriterFactory),
                    subtitleCueExtractor: subtitleCueExtractorFactory.Create(),
                    initialProgramDateTime: initialProgramDateTime
                );
            }).ToList();
        }

        private static string GetMediaManifestOutputPath(string manifestOutputPath)
        {
            var directory = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;
            return Path.Combine(directory, "media.m3u8");
        }

        private static string GetTsSegmentOutputPath(string manifestOutputPath)
        {
            var directory = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;
            return Path.Combine(directory, "{seqNum}.ts");
        }
    }
}
