using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpStreamDeletionService : IRtmpStreamDeletionService
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManager;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;

        public RtmpStreamDeletionService(
            IRtmpStreamManagerService rtmpStreamManager,
            IRtmpServerStreamEventDispatcher eventDispatcher)
        {
            _rtmpStreamManager = rtmpStreamManager;
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

            if (publishStreamContext == null || !_rtmpStreamManager.StopPublishing(publishStreamContext, out _))
                return;

            await _eventDispatcher.RtmpStreamUnpublishedAsync(streamContext.ClientContext, publishStreamContext.StreamPath);
        }

        private async ValueTask StopSubscribingStreamIfNeededAsync(IRtmpStreamContext streamContext)
        {
            var subscribeStreamContext = streamContext.SubscribeContext;

            if (subscribeStreamContext == null || !_rtmpStreamManager.StopSubscribing(subscribeStreamContext))
                return;

            await _eventDispatcher.RtmpStreamUnsubscribedAsync(streamContext.ClientContext, subscribeStreamContext.StreamPath);
        }
    }
}
