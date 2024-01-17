using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer
{
    public class SimpleFFmpegTransmuxerFactory : ITransmuxerFactory
    {
        private readonly SimpleFFmpegTransmuxerConfiguration _config;

        public SimpleFFmpegTransmuxerFactory(IOptions<SimpleFFmpegTransmuxerConfiguration> config)
        {
            _config = config.Value;
        }

        public Task<ITransmuxer> CreateAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            var process = new FFmpegTransmuxer(_config.FFmpegPath, _config.FFmpegTransmuxerArguments);
            return Task.FromResult<ITransmuxer>(process);
        }
    }
}
