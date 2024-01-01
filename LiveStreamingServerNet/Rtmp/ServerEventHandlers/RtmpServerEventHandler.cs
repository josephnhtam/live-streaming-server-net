using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.ServerEventHandlers
{
    public class RtmpServerEventHandler : IRtmpServerEventHandler
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManagerService;

        public RtmpServerEventHandler(IRtmpStreamManagerService rtmpStreamManagerService)
        {
            _rtmpStreamManagerService = rtmpStreamManagerService;
        }

        public void OnRtmpClientCreated(IRtmpClientPeerContext peerContext) { }

        public void OnRtmpClientDisposed(IRtmpClientPeerContext peerContext)
        {
            _rtmpStreamManagerService.StopPublishingStream(peerContext, out _);
            _rtmpStreamManagerService.StopSubscribingStream(peerContext);
        }
    }
}
