using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.ServerEventHandlers
{
    public class RtmpClientPeerDisposalEventHandler : IRtmpServerEventHandler
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManagerService;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;

        public RtmpClientPeerDisposalEventHandler(IRtmpStreamManagerService rtmpStreamManagerService, IRtmpUserControlMessageSenderService userControlMessageSender)
        {
            _rtmpStreamManagerService = rtmpStreamManagerService;
            _userControlMessageSender = userControlMessageSender;
        }

        public void OnRtmpClientCreated(IRtmpClientPeerContext peerContext) { }

        public void OnRtmpClientDisposed(IRtmpClientPeerContext peerContext)
        {
            StopPublishingStreamIfNeeded(peerContext);
            StopSubscribingStreamIfNeeded(peerContext);
        }

        private void StopPublishingStreamIfNeeded(IRtmpClientPeerContext peerContext)
        {
            if (_rtmpStreamManagerService.StopPublishingStream(peerContext, out var existingSubscriber))
            {
                _userControlMessageSender.SendStreamEofMessage(existingSubscriber, peerContext.PublishStreamContext!.Id);
            }
        }

        private void StopSubscribingStreamIfNeeded(IRtmpClientPeerContext peerContext)
        {
            _rtmpStreamManagerService.StopSubscribingStream(peerContext);
        }
    }
}
