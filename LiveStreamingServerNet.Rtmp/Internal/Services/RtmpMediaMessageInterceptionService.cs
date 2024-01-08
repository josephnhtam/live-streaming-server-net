using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpMediaMessageInterceptionService : IRtmpMediaMessageInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaMessageInterceptor> _interceptors;

        public RtmpMediaMessageInterceptionService(IEnumerable<IRtmpMediaMessageInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public async Task CachePictureAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnCachePicture(streamPath, mediaType, rentedBuffer, timestamp);
        }

        public async Task CacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);
        }

        public async Task ClearGroupOfPicturesCacheAsync(string streamPath)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnClearGroupOfPicturesCache(streamPath);
        }

        public async Task EnqueueMediaMessageAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnEnqueueMediaMessage(streamPath, mediaType, rentedBuffer, timestamp, isSkippable);
        }
    }
}
