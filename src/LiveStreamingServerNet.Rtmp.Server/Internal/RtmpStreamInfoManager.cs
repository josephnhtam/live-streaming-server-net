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
            var publishStreamContext = _service.GetPublishStreamContext(streamPath);

            if (publishStreamContext == null)
                return null;

            var subscriberContexts = _service.GetSubscribeStreamContexts(publishStreamContext.StreamPath);

            return new RtmpStreamInfo(
                   publishStreamContext.Stream.ClientContext.Client,
                   subscriberContexts.Select(x => x.Stream.ClientContext.Client).OfType<ISessionControl>().ToList(),
                   publishStreamContext);
        }

        public IList<IRtmpStreamInfo> GetStreamInfos()
        {
            return GetStreamPaths().Select(GetStreamInfo).Where(x => x != null).Cast<IRtmpStreamInfo>().ToList();
        }
    }
}
