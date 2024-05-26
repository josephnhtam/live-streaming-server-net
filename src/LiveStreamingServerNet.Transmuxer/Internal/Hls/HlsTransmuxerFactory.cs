using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Hls.Configurations;
using LiveStreamingServerNet.Transmuxer.Internal.Containers;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Marshal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal class HlsTransmuxerFactory : ITransmuxerFactory
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly IManifestWriter _manifestWriter;
        private readonly HlsTransmuxerConfiguration _config;
        private readonly ILogger<HlsTransmuxer> _logger;

        public HlsTransmuxerFactory(IHlsTransmuxerManager transmuxerManager, IManifestWriter manifestWriter, HlsTransmuxerConfiguration config, ILogger<HlsTransmuxer> logger)
        {
            _transmuxerManager = transmuxerManager;
            _manifestWriter = manifestWriter;
            _config = config;
            _logger = logger;
        }

        public async Task<ITransmuxer> CreateAsync(
            Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var outputPaths = await _config.OutputPathResolver.ResolveOutputPath(contextIdentifier, streamPath, streamArguments);

            var tsMuxer = new TsMuxer(outputPaths.TsFileOutputPath);

            var config = new HlsTransmuxer.Configuration(
                contextIdentifier,
                _config.Name,
                outputPaths.ManifestOutputPath,
                outputPaths.TsFileOutputPath,
                _config.SegmentListSize,
                _config.DeleteOutdatedSegments
            );

            return new HlsTransmuxer(_transmuxerManager, _manifestWriter, tsMuxer, config, _logger);
        }
    }
}
