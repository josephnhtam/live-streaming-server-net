using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpMediaMessageScraper : IRtmpMediaMessageInterceptor
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagBroadcasterService _mediaTagBroadcaster;
        private readonly IFlvMediaTagCacherService _mediaTagCacher;

        public RtmpMediaMessageScraper(
            IFlvStreamManagerService streamManager,
            IFlvMediaTagBroadcasterService mediaMessageManager,
            IFlvMediaTagCacherService mediaTagCacher)
        {
            _streamManager = streamManager;
            _mediaTagBroadcaster = mediaMessageManager;
            _mediaTagCacher = mediaTagCacher;
        }

        public async ValueTask OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            await _mediaTagCacher.CachePictureAsync(streamContext, mediaType, rentedBuffer, timestamp);
        }

        public async ValueTask OnClearGroupOfPicturesCache(string streamPath)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            await _mediaTagCacher.ClearGroupOfPicturesCacheAsync(streamContext);
        }

        public async ValueTask OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            await _mediaTagCacher.CacheSequenceHeaderAsync(streamContext, mediaType, sequenceHeader);

            var subscribers = _streamManager.GetSubscribers(streamPath);
            if (!subscribers.Any())
                return;

            var rentedBuffer = new RentedBuffer(sequenceHeader.Length, subscribers.Count);
            Array.Copy(sequenceHeader, rentedBuffer.Buffer, sequenceHeader.Length);

            await _mediaTagBroadcaster.BroadcastMediaTagAsync(streamContext, subscribers, mediaType, 0, false, rentedBuffer);
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
