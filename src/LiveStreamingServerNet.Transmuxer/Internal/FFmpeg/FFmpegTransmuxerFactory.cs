using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.FFmpeg.Configurations;

namespace LiveStreamingServerNet.Transmuxer.Internal.FFmpeg
{
    internal class FFmpegTransmuxerFactory : ITransmuxerFactory
    {
        private readonly FFmpegTransmuxerConfiguration _config;

        public FFmpegTransmuxerFactory(FFmpegTransmuxerConfiguration config)
        {
            _config = config;
        }

        public async Task<ITransmuxer> CreateAsync(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var config = new FFmpegTransmuxer.Configuration(
                contextIdentifier,
                _config.Name,
                _config.FFmpegPath,
                _config.FFmpegArguments,
                _config.GracefulShutdownTimeoutSeconds,
                await _config.OutputPathResolver.Invoke(contextIdentifier, streamPath, streamArguments)
            );

            return new FFmpegTransmuxer(config);
        }
    }
}
