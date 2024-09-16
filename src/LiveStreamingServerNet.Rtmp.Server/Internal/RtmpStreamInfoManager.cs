using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpStreamInfoManager : IRtmpStreamInfoManager
    {
        private readonly IRtmpStreamManagerService _service;

        public RtmpStreamInfoManager(IRtmpStreamManagerService service)
        {
            _service = service;
        }

        public IList<string> GetStreamPaths()
        {
            return _service.GetStreamPaths().ToList();
        }

        public IRtmpStreamInfo? GetStreamInfo(string streamPath)
        {
            var publisherContext = _service.GetPublishingClientContext(streamPath);
            var publishStreamContext = publisherContext?.PublishStreamContext;

            if (publishStreamContext == null)
                return null;

            var subscriberContexts = _service.GetSubscribers(publishStreamContext.StreamPath);

            return new RtmpStreamInfo(
                   publisherContext!.Client,
                   subscriberContexts.Select(x => x.Client).OfType<ISessionControl>().ToList(),
                   publishStreamContext);
        }

        public IList<IRtmpStreamInfo> GetStreamInfos()
        {
            return GetStreamPaths().Select(GetStreamInfo).Where(x => x != null).Cast<IRtmpStreamInfo>().ToList();
        }
    }
}
