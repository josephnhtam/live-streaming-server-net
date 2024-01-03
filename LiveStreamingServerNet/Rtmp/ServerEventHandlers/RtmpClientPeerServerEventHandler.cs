using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.ServerEventHandlers
{
    public class RtmpClientPeerServerEventHandler : IRtmpServerEventHandler
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManager;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;

        public RtmpClientPeerServerEventHandler(
            IRtmpStreamManagerService rtmpStreamManager,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpMediaMessageManagerService mediaMessageManager)
        {
            _rtmpStreamManager = rtmpStreamManager;
            _userControlMessageSender = userControlMessageSender;
            _commandMessageSender = commandMessageSender;
            _mediaMessageManager = mediaMessageManager;
        }

        public Task OnRtmpClientCreatedAsync(IRtmpClientPeerContext peerContext)
        {
            _mediaMessageManager.RegisterClientPeer(peerContext);
            return Task.CompletedTask;
        }

        public Task OnRtmpClientDisposedAsync(IRtmpClientPeerContext peerContext)
        {
            _mediaMessageManager.UnregisterClientPeer(peerContext);
            StopPublishingStreamIfNeeded(peerContext);
            StopSubscribingStreamIfNeeded(peerContext);
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
