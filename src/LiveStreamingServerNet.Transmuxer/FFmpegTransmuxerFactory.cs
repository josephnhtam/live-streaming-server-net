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

        public async Task<ITransmuxer> CreateAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            return new FFmpegTransmuxer(
                _config.TransmuxerIdentifier,
                _config.FFmpegPath,
                _config.FFmpegTransmuxerArguments,
                _config.GracefulShutdownTimeoutSeconds,
                await _config.OutputPathResolver.Invoke(streamPath, streamArguments));
        }
    }
}
