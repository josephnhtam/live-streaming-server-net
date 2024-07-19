using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
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
                   subscriberContexts.Select(x => x.Client).OfType<IClientControl>().ToList(),
                   publishStreamContext);
        }

        public IList<IRtmpStreamInfo> GetStreamInfos()
        {
            return GetStreamPaths().Select(GetStreamInfo).Where(x => x != null).Cast<IRtmpStreamInfo>().ToList();
        }
    }

    internal class RtmpStreamManager : IRtmpStreamManager
    {
        private readonly IRtmpStreamManagerService _service;

        public RtmpStreamManager(IRtmpStreamManagerService service)
        {
            _service = service;
        }

        public IList<string> GetStreamPaths()
        {
            return _service.GetStreamPaths().ToList();
        }

        public IRtmpStream? GetStream(string streamPath)
        {
            var publisherContext = _service.GetPublishingClientContext(streamPath);
            var publishStreamContext = publisherContext?.PublishStreamContext;

            if (publishStreamContext == null)
                return null;

            var subscriberContexts = _service.GetSubscribers(publishStreamContext.StreamPath);

            return new RtmpStream(
                   publisherContext!.Client,
                   subscriberContexts.Select(x => x.Client).OfType<IClientControl>().ToList(),
                   publishStreamContext);
        }

        public IList<IRtmpStream> GetStreams()
        {
            return GetStreamPaths().Select(GetStream).Where(x => x != null).Cast<IRtmpStream>().ToList();
        }
    }
}
