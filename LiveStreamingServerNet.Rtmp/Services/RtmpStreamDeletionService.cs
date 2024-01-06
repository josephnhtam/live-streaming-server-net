using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.Services
{
    internal class RtmpStreamDeletionService : IRtmpStreamDeletionService
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
            if (!_rtmpStreamManager.StopPublishingStream(peerContext, out var existingSubscriber))
                return;

            _userControlMessageSender.SendStreamEofMessage(existingSubscriber);
            SendStreamUnpublishNotify(existingSubscriber);
        }

        private void StopSubscribingStreamIfNeeded(IRtmpClientPeerContext peerContext)
        {
            if (!_rtmpStreamManager.StopSubscribingStream(peerContext))
                return;

            SendSubscriptionStoppedMessage(peerContext);
        }

        private void SendStreamUnpublishNotify(
            IList<IRtmpClientPeerContext> subscribers,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            foreach (var subscriberGroup in subscribers.GroupBy(x => x.StreamSubscriptionContext!.ChunkStreamId))
            {
                _commandMessageSender.SendOnStatusCommandMessage(
                    subscriberGroup.ToList(),
                    subscriberGroup.Key,
                    RtmpArgumentValues.Status,
                    RtmpStatusCodes.PlayUnpublishNotify,
                    "Stream is unpublished.",
                    amfEncodingType);
            }
        }

        private void SendSubscriptionStoppedMessage(
            IRtmpClientPeerContext subscriber,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                subscriber,
                subscriber.StreamSubscriptionContext!.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayUnpublishNotify,
                "Stream is stopped.",
                amfEncodingType);
        }
    }
}
