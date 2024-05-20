using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Hls.Configurations;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal class HlsTransmuxerFactory : ITransmuxerFactory
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;
        private readonly HlsTransmuxerConfiguration _config;

        public HlsTransmuxerFactory(IHlsTransmuxerManager transmuxerManager, HlsTransmuxerConfiguration config)
        {
            _transmuxerManager = transmuxerManager;
            _config = config;
        }

        public async Task<ITransmuxer> CreateAsync(
            Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var outputPaths = await _config.OutputPathResolver.ResolveOutputPath(contextIdentifier, streamPath, streamArguments);

            return new HlsTransmuxer(
                _config.Name,
                contextIdentifier,
                _transmuxerManager,
                outputPaths.ManifestOutputPath,
                outputPaths.TsFileOutputPath
            );
        }
    }
}
