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

        public async ValueTask CloseStreamAsync(IRtmpStreamContext streamContext)
        {
            await StopPublishingStreamIfNeededAsync(streamContext);
            await StopSubscribingStreamIfNeededAsync(streamContext);
        }

        public async ValueTask DeleteStreamAsync(IRtmpStreamContext streamContext)
        {
            await CloseStreamAsync(streamContext);

            streamContext.ClientContext.RemoveStreamContext(streamContext.StreamId);
        }

        private async ValueTask StopPublishingStreamIfNeededAsync(IRtmpStreamContext streamContext)
        {
            var publishStreamContext = streamContext.PublishContext;

            if (publishStreamContext == null || !_rtmpStreamManager.StopPublishing(publishStreamContext, out var existingSubscriber))
                return;

            SendStreamUnpublishNotify(existingSubscriber.AsReadOnly());
            _userControlMessageSender.SendStreamEofMessage(existingSubscriber.AsReadOnly());
            await _eventDispatcher.RtmpStreamUnpublishedAsync(streamContext.ClientContext, publishStreamContext.StreamPath);
        }

        private async ValueTask StopSubscribingStreamIfNeededAsync(IRtmpStreamContext streamContext)
        {
            var subscribeStreamContext = streamContext.SubscribeContext;

            if (subscribeStreamContext == null || !_rtmpStreamManager.StopSubscribing(subscribeStreamContext))
                return;

            await _eventDispatcher.RtmpStreamUnsubscribedAsync(streamContext.ClientContext, subscribeStreamContext.StreamPath);
        }

        private void SendStreamUnpublishNotify(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            foreach (var subscriberGroup in subscribeStreamContexts.GroupBy(x => x.StreamContext.StreamId))
            {
                _commandMessageSender.SendOnStatusCommandMessage(
                    subscriberGroup.Select(x => x.StreamContext.ClientContext).ToList(),
                    subscriberGroup.Key,
                    RtmpStatusLevels.Status,
                    RtmpStreamStatusCodes.PlayUnpublishNotify,
                    "Stream is unpublished.",
                    amfEncodingType);
            }
        }
    }
}
