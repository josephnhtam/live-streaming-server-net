using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.ServerEventHandlers
{
    public class RtmpClientPeerDisposalEventHandler : IRtmpServerEventHandler
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManagerService;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSenderService;

        public RtmpClientPeerDisposalEventHandler(
            IRtmpStreamManagerService rtmpStreamManagerService,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSenderService)
        {
            _rtmpStreamManagerService = rtmpStreamManagerService;
            _userControlMessageSender = userControlMessageSender;
            _commandMessageSenderService = commandMessageSenderService;
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
                _userControlMessageSender.SendStreamEofMessage(existingSubscriber);
                _commandMessageSenderService.SendStreamUnpublishNotify(existingSubscriber);
            }
        }

        private void StopSubscribingStreamIfNeeded(IRtmpClientPeerContext peerContext)
        {
            _rtmpStreamManagerService.StopSubscribingStream(peerContext);
        }
    }
}
