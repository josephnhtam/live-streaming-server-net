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
        private readonly ILogger<AdaptiveHlsTranscoder> _logger;

        public AdaptiveHlsTranscoderFactory(
            IServiceProvider services,
            IHlsCleanupManager cleanupManager,
            IHlsPathRegistry pathRegistry,
            AdaptiveHlsTranscoderConfiguration config,
            ILogger<AdaptiveHlsTranscoder> logger)
        {
            _services = services;
            _cleanupManager = cleanupManager;
            _pathRegistry = pathRegistry;
            _config = config;
            _logger = logger;
        }

        public async Task<IStreamProcessor?> CreateAsync(
            ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                if (!await _config.Condition.IsEnabled(_services, streamPath, streamArguments))
                    return null;

                var outputPath = await _config.OutputPathResolver.ResolveOutputPath(
                    _services, contextIdentifier, streamPath, streamArguments);

                var config = new AdaptiveHlsTranscoder.Configuration(
                    ContextIdentifier: contextIdentifier,
                    Name: _config.Name,
                    ManifestOutputPath: outputPath,

                    FFmpegPath: _config.FFmpegPath,
                    FFprobeGracefulShutdownTimeoutSeconds: _config.FFmpegGracefulShutdownTimeoutSeconds,

                    FFprobePath: _config.FFprobePath,
                    FFmpegGracefulTerminationSeconds: _config.FFprobeGracefulShutdownTimeoutSeconds,

                    HlsOptions: _config.HlsOptions,
                    PerformanceOptions: _config.PerformanceOptions,
                    DownsamplingFilters: _config.DownsamplingFilters.ToArray(),

                    VideoEncodingArguments: _config.VideoEncodingArguments,
                    AudioEncodingArguments: _config.AudioEncodingArguments,

                    VideoDecodingArguments: _config.VideoDecodingArguments,
                    AudioDecodingArguments: _config.AudioDecodingArguments,

                    VideoFilters: _config.VideoFilters?.ToArray(),
                    AudioFilters: _config.AudioFilters?.ToArray(),

                    AdditionalInputs: _config.AdditionalInputs?.ToArray(),
                    AdditionalComplexFilters: _config.AdditionalComplexFilters?.ToArray(),

                    CleanupDelay: _config.HlsOptions.DeleteOutdatedSegments ? _config.CleanupDelay : null
                );

                return new AdaptiveHlsTranscoder(_cleanupManager, _pathRegistry, config, _logger);
            }
            catch
            {
                return null;
            }
        }
    }
}
