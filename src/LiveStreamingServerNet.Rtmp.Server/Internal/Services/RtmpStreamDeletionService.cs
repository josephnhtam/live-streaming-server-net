using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpStreamDeletionService : IRtmpStreamDeletionService
    {
        private readonly IRtmpStreamManagerService _rtmpStreamManager;

        public RtmpStreamDeletionService(IRtmpStreamManagerService rtmpStreamManager)
        {
            _rtmpStreamManager = rtmpStreamManager;
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

            if (publishStreamContext == null)
                return;

            await _rtmpStreamManager.StopPublishingAsync(publishStreamContext);
        }

        private async ValueTask StopSubscribingStreamIfNeededAsync(IRtmpStreamContext streamContext)
        {
            var subscribeStreamContext = streamContext.SubscribeContext;

            if (subscribeStreamContext == null)
                return;

            await _rtmpStreamManager.StopSubscribingAsync(subscribeStreamContext);
        }
    }
}
