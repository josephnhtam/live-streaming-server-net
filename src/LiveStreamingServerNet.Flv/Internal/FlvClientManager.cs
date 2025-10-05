using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvClientManager : IFlvClientManager
    {
        private readonly IFlvStreamManagerService _streamManager;

        public FlvClientManager(IFlvStreamManagerService streamManager)
        {
            _streamManager = streamManager;
        }

        public IList<IFlvClientHandle> GetFlvClients(string streamPath)
        {
            return _streamManager.GetSubscribers(streamPath)
                .Select(c => new FlvClientHandle(c))
                .OfType<IFlvClientHandle>()
                .ToList();
        }
    }
}
