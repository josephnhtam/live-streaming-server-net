using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpMediaMessageScraper : IRtmpMediaMessageInterceptor
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;

        public RtmpMediaMessageScraper(
            IFlvStreamManagerService streamManager,
            IFlvMediaTagBroadcasterService mediaMessageManager)
        {
            _streamManager = streamManager;
            _mediaTagBroadcaster = mediaMessageManager;
        }

        public async ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            var subscribers = _streamManager.GetSubscribers(streamPath);
            if (!subscribers.Any())
                return;

            await _mediaTagBroadcaster.BroadcastMediaTagAsync(streamContext, subscribers, mediaType, timestamp, isSkippable, rentedBuffer);
        }
    }
}
