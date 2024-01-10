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
        private readonly IFlvMediaMessageManagerService _mediaMessageManager;

        public RtmpMediaMessageScraper(IFlvStreamManagerService streamManager, IFlvMediaMessageManagerService mediaMessageManager)
        {
            _streamManager = streamManager;
            _mediaMessageManager = mediaMessageManager;
        }

        public async Task OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            await _mediaMessageManager.CachePictureAsync(streamContext, mediaType, rentedBuffer, timestamp);
        }

        public async Task OnClearGroupOfPicturesCache(string streamPath)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            await _mediaMessageManager.ClearGroupOfPicturesCacheAsync(streamContext);
        }

        public async Task OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            await _mediaMessageManager.CacheSequenceHeaderAsync(streamContext, mediaType, sequenceHeader);

            var subscribers = _streamManager.GetSubscribers(streamPath);
            if (!subscribers.Any())
                return;

            var rentedBuffer = new RentedBuffer(sequenceHeader.Length, subscribers.Count);
            Array.Copy(sequenceHeader, rentedBuffer.Buffer, sequenceHeader.Length);

            await _mediaMessageManager.EnqueueMediaMessageAsync(streamContext, subscribers, mediaType, 0, false, rentedBuffer);
        }

        public async Task OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);
            if (streamContext == null)
                return;

            var subscribers = _streamManager.GetSubscribers(streamPath);
            if (!subscribers.Any())
                return;

            await _mediaMessageManager.EnqueueMediaMessageAsync(streamContext, subscribers, mediaType, timestamp, isSkippable, rentedBuffer);
        }
    }
}
