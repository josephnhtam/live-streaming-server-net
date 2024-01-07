using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
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

        public void DeleteStream(IRtmpClientContext clientContext)
        {
            StopPublishingStreamIfNeeded(clientContext);
            StopSubscribingStreamIfNeeded(clientContext);
            clientContext.DeleteStream();
        }

        private void StopPublishingStreamIfNeeded(IRtmpClientContext clientContext)
        {
            if (!_rtmpStreamManager.StopPublishingStream(clientContext, out var existingSubscriber))
                return;

            _userControlMessageSender.SendStreamEofMessage(existingSubscriber);
            SendStreamUnpublishNotify(existingSubscriber);
        }

        private void StopSubscribingStreamIfNeeded(IRtmpClientContext clientContext)
        {
            if (!_rtmpStreamManager.StopSubscribingStream(clientContext))
                return;

            SendSubscriptionStoppedMessage(clientContext);
        }

        private void SendStreamUnpublishNotify(
            IList<IRtmpClientContext> subscribers,
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
            IRtmpClientContext subscriber,
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
