using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.Services
{
    public class RtmpStreamDeletionService : IRtmpStreamDeletionService
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManager;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;

        public RtmpStreamDeletionService(
            IRtmpStreamManagerService rtmpStreamManager,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSender)
        {
            _rtmpStreamManager = rtmpStreamManager;
            _userControlMessageSender = userControlMessageSender;
            _commandMessageSender = commandMessageSender;
        }

        public Task DeleteStream(IRtmpClientPeerContext peerContext)
        {
            StopPublishingStreamIfNeeded(peerContext);
            StopSubscribingStreamIfNeeded(peerContext);
            peerContext.DeleteStream();
            return Task.CompletedTask;
        }

        private void StopPublishingStreamIfNeeded(IRtmpClientPeerContext peerContext)
        {
            if (_rtmpStreamManager.StopPublishingStream(peerContext, out var existingSubscriber))
            {
                _userControlMessageSender.SendStreamEofMessage(existingSubscriber);
                _commandMessageSender.SendStreamUnpublishNotify(existingSubscriber);
            }
        }

        private void StopSubscribingStreamIfNeeded(IRtmpClientPeerContext peerContext)
        {
            _rtmpStreamManager.StopSubscribingStream(peerContext);
        }
    }
}
