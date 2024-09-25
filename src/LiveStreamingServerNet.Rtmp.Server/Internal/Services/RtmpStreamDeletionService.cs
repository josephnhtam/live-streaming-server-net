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
            if (stream.PublishContext == null || !_rtmpStreamManager.StopPublishing(stream.PublishContext, out var existingSubscriber))
                return;

            _userControlMessageSender.SendStreamEofMessage(existingSubscriber.AsReadOnly());
            SendStreamUnpublishNotify(existingSubscriber.AsReadOnly());
            await _eventDispatcher.RtmpStreamUnpublishedAsync(stream.PublishContext.Stream.ClientContext, stream.PublishContext.StreamPath);
        }

        private async ValueTask StopSubscribingStreamIfNeededAsync(IRtmpStream stream)
        {
            if (stream.SubscribeContext == null || !_rtmpStreamManager.StopSubscribing(stream.SubscribeContext))
                return;

            SendSubscriptionStoppedMessage(stream.SubscribeContext);
            await _eventDispatcher.RtmpStreamUnsubscribedAsync(stream.SubscribeContext.Stream.ClientContext, stream.SubscribeContext.StreamPath);
        }

        private void SendStreamUnpublishNotify(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            foreach (var subscriberGroup in
                subscribeStreamContexts.Where(x => x.Stream?.SubscribeContext != null)
                           .GroupBy(x => x.Stream!.Id))
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
