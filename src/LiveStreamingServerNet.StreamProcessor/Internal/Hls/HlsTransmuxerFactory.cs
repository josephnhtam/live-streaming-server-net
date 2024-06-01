using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Internal.Containers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Marshal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls
{
    internal class HlsTransmuxerFactory : IStreamProcessorFactory
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IManifestWriter _manifestWriter;
        private readonly HlsTransmuxerConfiguration _config;
        private readonly ILogger<HlsTransmuxer> _logger;
        private readonly IBufferPool? _bufferPool;

        public HlsTransmuxerFactory(
            IHlsTransmuxerManager transmuxerManager,
            IManifestWriter manifestWriter,
            HlsTransmuxerConfiguration config,
            ILogger<HlsTransmuxer> logger,
            IBufferPool? bufferPool)
        {
            _transmuxerManager = transmuxerManager;
            _manifestWriter = manifestWriter;
            _config = config;
            _logger = logger;
            _bufferPool = bufferPool;
        }

        public async Task<IStreamProcessor?> CreateAsync(
            IClientHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                var outputPaths = await _config.OutputPathResolver.ResolveOutputPath(contextIdentifier, streamPath, streamArguments);

                var tsMuxer = new TsMuxer(outputPaths.TsFileOutputPath, _bufferPool);

                var config = new HlsTransmuxer.Configuration(
                    contextIdentifier,
                    _config.Name,
                    outputPaths.ManifestOutputPath,
                    outputPaths.TsFileOutputPath,
                    _config.SegmentListSize,
                    _config.DeleteOutdatedSegments,
                    _config.MaxSegmentBufferSize,
                    _config.AudioOnlySegmentDuration
                );

                return new HlsTransmuxer(streamPath, client, _transmuxerManager, _manifestWriter, tsMuxer, config, _logger);
            }
            catch
            {
                return null;
            }
        }
    }
}
