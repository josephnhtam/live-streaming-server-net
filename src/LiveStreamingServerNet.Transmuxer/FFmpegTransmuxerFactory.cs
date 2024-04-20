using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;

namespace LiveStreamingServerNet.Transmuxer
{
    public class FFmpegTransmuxerFactory : ITransmuxerFactory
    {
        private readonly FFmpegTransmuxerFactoryConfiguration _config;

        public FFmpegTransmuxerFactory(FFmpegTransmuxerFactoryConfiguration config)
        {
            _config = config;
        }

        public async Task<ITransmuxer> CreateAsync(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return new FFmpegTransmuxer(
                contextIdentifier,
                _config.Name,
                _config.FFmpegPath,
                _config.FFmpegArguments,
                _config.GracefulShutdownTimeoutSeconds,
                await _config.OutputPathResolver.Invoke(contextIdentifier, streamPath, streamArguments));
        }
    }
}
