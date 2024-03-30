using LiveStreamingServerNet.KubernetesPod.Redis.Configurations;
using LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.KubernetesPod.Redis.Internal.Services
{
    internal class StreamKeyProvider : IStreamKeyProvider
    {
        private readonly RedisStoreConfiguration _config;

        public StreamKeyProvider(IOptions<RedisStoreConfiguration> config)
        {
            _config = config.Value;
        }

        public string ResolveStreamKey(string streamPath)
        {
            return _config.StreamKeyPrefix + streamPath;
        }
    }
}
