using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer
{
    public class OutputPathResolver : IOutputPathResolver
    {
        private readonly TransmuxerConfiguration _config;

        public OutputPathResolver(IOptions<TransmuxerConfiguration> config)
        {
            _config = config.Value;
        }

        public Task<string> ResolveOutputPathAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            return Task.FromResult(Path.Combine(_config.OutputDirectoryPath, Guid.NewGuid().ToString()));
        }
    }
}
