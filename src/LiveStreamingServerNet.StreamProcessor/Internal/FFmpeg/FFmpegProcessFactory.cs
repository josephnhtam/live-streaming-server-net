using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg
{
    internal class FFmpegProcessFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly FFmpegProcessConfiguration _config;
        private readonly IFFmpegProcessConfigurationResolver? _resolver;
        private readonly ILogger<FFmpegProcess> _logger;

        public FFmpegProcessFactory(
            IServiceProvider services,
            FFmpegProcessConfiguration config,
            IFFmpegProcessConfigurationResolver? resolver,
            ILogger<FFmpegProcess> logger)
        {
            _services = services;
            _config = config;
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<IStreamProcessor?> CreateAsync(ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var config = await ResolveConfigAsync(contextIdentifier, streamPath, streamArguments).ConfigureAwait(false);

            if (!await config.Condition.IsEnabled(_services, streamPath, streamArguments).ConfigureAwait(false))
                return null;

            var outputPath = await _config.OutputPathResolver.ResolveOutputPath(
                _services, contextIdentifier, streamPath, streamArguments).ConfigureAwait(false);

            var processConfig = new FFmpegProcess.Configuration(
                contextIdentifier,
                config.Name,
                config.FFmpegPath,
                config.FFmpegArguments,
                config.GracefulShutdownTimeoutSeconds,
                outputPath
            );

            return new FFmpegProcess(streamPath, processConfig, _logger);
        }

        private async ValueTask<FFmpegProcessConfiguration> ResolveConfigAsync(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (_resolver == null)
                return _config;

            return await _resolver.ResolveAsync(_services, contextIdentifier, streamPath, streamArguments).ConfigureAwait(false) ?? _config;
        }
    }
}
