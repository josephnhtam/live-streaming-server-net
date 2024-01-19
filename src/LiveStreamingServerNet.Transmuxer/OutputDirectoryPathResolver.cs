using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer
{
    public class OutputDirectoryPathResolver : IOutputDirectoryPathResolver
    {
        private readonly RemuxingConfiguration _config;

        public OutputDirectoryPathResolver(IOptions<RemuxingConfiguration> config)
        {
            _config = config.Value;
        }

        public Task<string> ResolveOutputDirectoryPathAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            return Task.FromResult(Path.Combine(_config.OutputDirectoryPath, Guid.NewGuid().ToString()));
        }
    }
}
