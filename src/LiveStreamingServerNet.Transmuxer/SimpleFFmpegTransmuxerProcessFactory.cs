using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer
{
    public class SimpleFFmpegTransmuxerProcessFactory : ITransmuxerProcessFactory
    {
        private readonly SimpleFFmpegTransmuxerConfiguration _config;

        public SimpleFFmpegTransmuxerProcessFactory(IOptions<SimpleFFmpegTransmuxerConfiguration> config)
        {
            _config = config.Value;
        }

        public Task<ITransmuxerProcess> CreateAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            var process = new FFmpegTransmuxerProcess(_config.FFmpegPath, _config.FFmpegTransmuxerArguments);
            return Task.FromResult<ITransmuxerProcess>(process);
        }
    }
}
