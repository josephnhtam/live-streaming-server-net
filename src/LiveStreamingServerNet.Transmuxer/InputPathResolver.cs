using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Transmuxer
{
    public class InputPathResolver : IInputPathResolver
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly TransmuxerConfiguration _config;

        public InputPathResolver(IRtmpServerContext serverContext, IOptions<TransmuxerConfiguration> config)
        {
            _config = config.Value;
            _serverContext = serverContext;
        }

        public Task<string> ResolveInputPathAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            var code = _serverContext.AuthCode;
            var inputPath = new Uri(new(_config.InputBasePath), streamPath);
            return Task.FromResult(QueryHelpers.AddQueryString(inputPath.ToString(), "code", code));
        }
    }
}
