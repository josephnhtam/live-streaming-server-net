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
        private readonly ILogger<FFmpegProcess> _logger;

        public FFmpegProcessFactory(IServiceProvider services, FFmpegProcessConfiguration config, ILogger<FFmpegProcess> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;
        }

        public async Task<IStreamProcessor?> CreateAsync(ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!await _config.Condition.IsEnabled(_services, streamPath, streamArguments))
                return null;

            var outputPath = await _config.OutputPathResolver.ResolveOutputPath(
                _services, contextIdentifier, streamPath, streamArguments);

            var config = new FFmpegProcess.Configuration(
                contextIdentifier,
                _config.Name,
                _config.FFmpegPath,
                _config.FFmpegArguments,
                _config.GracefulShutdownTimeoutSeconds,
                outputPath
            );

            return new FFmpegProcess(config, _logger);
        }
    }
}
