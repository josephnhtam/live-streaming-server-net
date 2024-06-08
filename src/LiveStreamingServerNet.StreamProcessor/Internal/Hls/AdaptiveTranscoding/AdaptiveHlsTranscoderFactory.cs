using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding
{
    internal class AdaptiveHlsTranscoderFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly AdaptiveHlsTranscoderConfiguration _config;
        private readonly ILogger<AdaptiveHlsTranscoder> _logger;

        public AdaptiveHlsTranscoderFactory(
            IServiceProvider services,
            IHlsCleanupManager cleanupManager,
            AdaptiveHlsTranscoderConfiguration config,
            ILogger<AdaptiveHlsTranscoder> logger)
        {
            _services = services;
            _cleanupManager = cleanupManager;
            _config = config;
            _logger = logger;
        }

        public async Task<IStreamProcessor?> CreateAsync(
            IClientHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                if (!await _config.Condition.IsEnabled(_services, streamPath, streamArguments))
                    return null;

                var outputPath = await _config.OutputPathResolver.ResolveOutputPath(
                    _services, contextIdentifier, streamPath, streamArguments);

                var config = new AdaptiveHlsTranscoder.Configuration(
                    contextIdentifier,
                    _config.Name,
                    outputPath,

                    _config.FFmpegPath,
                    _config.FFmpegGracefulShutdownTimeoutSeconds,

                    _config.FFprobePath,
                    _config.FFprobeGracefulShutdownTimeoutSeconds,

                    _config.HlsOptions,
                    _config.PerformanceOptions,
                    _config.DownsamplingFilters.ToArray(),

                    _config.VideoEncodingArguments,
                    _config.AudioEncodingArguments,

                    _config.VideoDecodingArguments,
                    _config.AudioDecodingArguments,

                    _config.HlsOptions.DeleteOutdatedSegments ? _config.CleanupDelay : null
                );

                return new AdaptiveHlsTranscoder(_cleanupManager, config, _logger);
            }
            catch
            {
                return null;
            }
        }
    }
}
