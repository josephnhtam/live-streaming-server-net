using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvClientHandle : IFlvClientHandle
    {
        public string ClientId => _client.ClientId;
        public string StreamPath => _client.StreamPath;
        public IReadOnlyDictionary<string, string> StreamArguments => _client.StreamArguments;
        public IFlvRequest Request => _client.Request;

        private readonly IFlvClient _client;

        public FlvClientHandle(IFlvClient client)
        {
            _client = client;
        }

        public void Stop()
        {
            _client.Stop();
        }
    }
}
