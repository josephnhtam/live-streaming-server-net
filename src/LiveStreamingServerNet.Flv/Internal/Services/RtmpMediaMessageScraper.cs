using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
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

        public async ValueTask OnReceiveMediaMessageAsync(uint clientId, string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            streamContext.SetReady();
            streamContext.UpdateTimestamp(timestamp, mediaType);

            var subscribers = _streamManager.GetSubscribers(streamPath);
            if (!subscribers.Any())
                return;

            var currentTimestamp = mediaType switch
            {
                MediaType.Audio => streamContext.AudioTimestamp,
                MediaType.Video => streamContext.VideoTimestamp,
                _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
            };

            await _mediaTagBroadcaster.BroadcastMediaTagAsync(streamContext, subscribers, mediaType, currentTimestamp, isSkippable, rentedBuffer).ConfigureAwait(false);
        }
    }
}
