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

            if (publishStreamContext == null || publishStreamContext.StreamContext == null)
                return null;

            var subscribeStreamContexts = _service.GetSubscribeStreamContexts(publishStreamContext.StreamPath);

            return new RtmpStreamInfo(
                   publishStreamContext.StreamContext.ClientContext.Client,
                   subscribeStreamContexts.Select(x => x.StreamContext.ClientContext.Client).OfType<ISessionControl>().ToList(),
                   publishStreamContext);
        }

        public IList<IRtmpStreamInfo> GetStreamInfos()
        {
            return GetStreamPaths().Select(GetStreamInfo).Where(x => x != null).Cast<IRtmpStreamInfo>().ToList();
        }
    }
}
