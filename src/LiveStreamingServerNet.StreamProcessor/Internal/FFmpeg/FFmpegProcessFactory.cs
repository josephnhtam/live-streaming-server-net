using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;

namespace LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg
{
    internal class FFmpegProcessFactory : IStreamProcessorFactory
    {
        private readonly IServiceProvider _services;
        private readonly FFmpegProcessConfiguration _config;

        public FFmpegProcessFactory(IServiceProvider services, FFmpegProcessConfiguration config)
        {
            _services = services;
            _config = config;
        }

        public async Task<IStreamProcessor?> CreateAsync(IClientHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
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

            return new FFmpegProcess(config);
        }
    }
}
