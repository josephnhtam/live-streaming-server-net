using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding
{
    internal class AdaptiveHlsTranscoderFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly IHlsPathRegistry _pathRegistry;
        private readonly AdaptiveHlsTranscoderConfiguration _config;
        private readonly IAdaptiveHlsTranscoderConfigurationResolver? _resolver;
        private readonly ILogger<AdaptiveHlsTranscoder> _logger;

        public AdaptiveHlsTranscoderFactory(
            IServiceProvider services,
            IHlsCleanupManager cleanupManager,
            IHlsPathRegistry pathRegistry,
            AdaptiveHlsTranscoderConfiguration config,
            IAdaptiveHlsTranscoderConfigurationResolver? resolver,
            ILogger<AdaptiveHlsTranscoder> logger)
        {
            _services = services;
            _cleanupManager = cleanupManager;
            _pathRegistry = pathRegistry;
            _config = config;
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<IStreamProcessor?> CreateAsync(
            ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var config = await ResolveConfigAsync(contextIdentifier, streamPath, streamArguments).ConfigureAwait(false);

            if (!await config.Condition.IsEnabled(_services, streamPath, streamArguments).ConfigureAwait(false))
                return null;

            var outputPath = await config.OutputPathResolver.ResolveOutputPath(
                _services, contextIdentifier, streamPath, streamArguments).ConfigureAwait(false);

            var transcoderConfig = new AdaptiveHlsTranscoder.Configuration(
                ContextIdentifier: contextIdentifier,
                Name: config.Name,
                ManifestOutputPath: outputPath,
                FFmpegPath: config.FFmpegPath,
                FFprobeGracefulShutdownTimeoutSeconds: config.FFmpegGracefulShutdownTimeoutSeconds,
                FFprobePath: config.FFprobePath,
                FFmpegGracefulTerminationSeconds: config.FFprobeGracefulShutdownTimeoutSeconds,
                HlsOptions: config.HlsOptions,
                PerformanceOptions: config.PerformanceOptions,
                DownsamplingFilters: config.DownsamplingFilters.ToArray(),
                VideoEncodingArguments: config.VideoEncodingArguments,
                AudioEncodingArguments: config.AudioEncodingArguments,
                VideoDecodingArguments: config.VideoDecodingArguments,
                AudioDecodingArguments: config.AudioDecodingArguments,
                VideoFilters: config.VideoFilters?.ToArray(),
                AudioFilters: config.AudioFilters?.ToArray(),
                AdditionalInputs: config.AdditionalInputs?.ToArray(),
                AdditionalComplexFilters: config.AdditionalComplexFilters?.ToArray(),
                CleanupDelay: config.HlsOptions.DeleteOutdatedSegments ? config.CleanupDelay : null
            );

            return new AdaptiveHlsTranscoder(streamPath, _cleanupManager, _pathRegistry, transcoderConfig, _logger);
        }

        private async ValueTask<AdaptiveHlsTranscoderConfiguration> ResolveConfigAsync(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (_resolver == null)
                return _config;

            return await _resolver.ResolveAsync(_services, contextIdentifier, streamPath, streamArguments).ConfigureAwait(false) ?? _config;
        }
    }
}
