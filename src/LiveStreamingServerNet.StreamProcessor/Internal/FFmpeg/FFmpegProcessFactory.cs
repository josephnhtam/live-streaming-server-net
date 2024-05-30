using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;

namespace LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg
{
    internal class FFmpegProcessFactory : IStreamProcessorFactory
    {
        private readonly FFmpegProcessConfiguration _config;

        public FFmpegProcessFactory(FFmpegProcessConfiguration config)
        {
            _config = config;
        }

        public async Task<IStreamProcessor?> CreateAsync(IClientHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var config = new FFmpegProcess.Configuration(
                contextIdentifier,
                _config.Name,
                _config.FFmpegPath,
                _config.FFmpegArguments,
                _config.GracefulShutdownTimeoutSeconds,
                await _config.OutputPathResolver.Invoke(contextIdentifier, streamPath, streamArguments)
            );

            return new FFmpegProcess(config);
        }
    }
}
