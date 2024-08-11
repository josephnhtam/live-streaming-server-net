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
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;

        public RtmpStreamDeletionService(
            IRtmpStreamManagerService rtmpStreamManager,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpServerStreamEventDispatcher eventDispatcher)
        {
            _rtmpStreamManager = rtmpStreamManager;
            _userControlMessageSender = userControlMessageSender;
            _commandMessageSender = commandMessageSender;
            _eventDispatcher = eventDispatcher;
        }

        public async ValueTask DeleteStreamAsync(IRtmpClientSessionContext clientContext)
        {
            await StopPublishingStreamIfNeededAsync(clientContext);
            await StopSubscribingStreamIfNeededAsync(clientContext);

            clientContext.DeleteStream();
        }

        private async ValueTask StopPublishingStreamIfNeededAsync(IRtmpClientSessionContext clientContext)
        {
            if (!_rtmpStreamManager.StopPublishingStream(clientContext, out var existingSubscriber))
                return;

            _userControlMessageSender.SendStreamEofMessage(existingSubscriber.AsReadOnly());
            SendStreamUnpublishNotify(existingSubscriber.AsReadOnly());
            await _eventDispatcher.RtmpStreamUnpublishedAsync(clientContext, clientContext.PublishStreamContext!.StreamPath);
        }

        private async ValueTask StopSubscribingStreamIfNeededAsync(IRtmpClientSessionContext clientContext)
        {
            if (!_rtmpStreamManager.StopSubscribingStream(clientContext))
                return;

            SendSubscriptionStoppedMessage(clientContext);
            await _eventDispatcher.RtmpStreamUnsubscribedAsync(clientContext, clientContext.StreamSubscriptionContext!.StreamPath);
        }

        private void SendStreamUnpublishNotify(
            IReadOnlyList<IRtmpClientSessionContext> subscribers,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            foreach (var subscriberGroup in
                subscribers.Where(x => x.StreamSubscriptionContext != null)
                           .GroupBy(x => x.StreamSubscriptionContext!.ChunkStreamId))
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
            IRtmpClientSessionContext subscriber,
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
