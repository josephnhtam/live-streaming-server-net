using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer
{
    public class FFmpegTransmuxerFactory : ITransmuxerFactory
    {
        private readonly FFmpegTransmuxerFactoryConfiguration _config;

        public FFmpegTransmuxerFactory(FFmpegTransmuxerFactoryConfiguration config)
        {
            _config = config;
        }

        public Task<ITransmuxer> CreateAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            var process = new FFmpegTransmuxer(
                _config.TransmuxerIdentifier,
                _config.FFmpegPath,
                _config.FFmpegTransmuxerArguments,
                _config.OutputFileName,
                _config.GracefulShutdownTimeoutSeconds);

            return Task.FromResult<ITransmuxer>(process);
        }
    }
}
