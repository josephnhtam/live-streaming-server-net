using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvStreamInfoManager : IFlvStreamInfoManager
    {
        private readonly IFlvStreamManagerService _streamManager;

        public FlvStreamInfoManager(IFlvStreamManagerService streamManager)
        {
            _streamManager = streamManager;
        }

        public IList<string> GetStreamPaths()
        {
            return _streamManager.GetStreamPaths();
        }

        public IFlvStreamInfo? GetStreamInfo(string streamPath)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return null;

            var subscribers = _streamManager.GetSubscribers(streamPath);
            var subscriberHandles = subscribers.Select(x => new FlvClientHandle(x)).OfType<IFlvClientHandle>().ToList();
            return new FlvStreamInfo(streamContext, subscriberHandles);
        }

        public IList<IFlvStreamInfo> GetStreamInfos()
        {
            return GetStreamPaths().Select(GetStreamInfo).Where(x => x != null).OfType<IFlvStreamInfo>().ToList();
        }
    }
}
