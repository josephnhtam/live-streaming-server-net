using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
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

        public async ValueTask DeleteStreamAsync(IRtmpStream stream)
        {
            await StopPublishingStreamIfNeededAsync(stream);
            await StopSubscribingStreamIfNeededAsync(stream);

            stream.Delete();
        }

        private async ValueTask StopPublishingStreamIfNeededAsync(IRtmpStream stream)
        {
            var publishStreamContext = stream.PublishContext;

            if (publishStreamContext == null || !_rtmpStreamManager.StopPublishing(publishStreamContext, out var existingSubscriber))
                return;

            _userControlMessageSender.SendStreamEofMessage(existingSubscriber.AsReadOnly());
            SendStreamUnpublishNotify(existingSubscriber.AsReadOnly());
            await _eventDispatcher.RtmpStreamUnpublishedAsync(stream.ClientContext, publishStreamContext.StreamPath);
        }

        private async ValueTask StopSubscribingStreamIfNeededAsync(IRtmpStream stream)
        {
            var subscribeStreamContext = stream.SubscribeContext;

            if (subscribeStreamContext == null || !_rtmpStreamManager.StopSubscribing(subscribeStreamContext))
                return;

            SendSubscriptionStoppedMessage(subscribeStreamContext);
            await _eventDispatcher.RtmpStreamUnsubscribedAsync(stream.ClientContext, subscribeStreamContext.StreamPath);
        }

        private void SendStreamUnpublishNotify(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            foreach (var subscriberGroup in subscribeStreamContexts.GroupBy(x => x.Stream.Id))
            {
                _commandMessageSender.SendOnStatusCommandMessage(
                    subscriberGroup.Select(x => x.Stream.ClientContext).ToList(),
                    subscriberGroup.Key,
                    RtmpArgumentValues.Status,
                    RtmpStatusCodes.PlayUnpublishNotify,
                    "Stream is unpublished.",
                    amfEncodingType);
            }
        }

        private void SendSubscriptionStoppedMessage(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                subscribeStreamContext.Stream.ClientContext,
                subscribeStreamContext.Stream.Id,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayUnpublishNotify,
                "Stream is stopped.",
                amfEncodingType);
        }
    }
}
