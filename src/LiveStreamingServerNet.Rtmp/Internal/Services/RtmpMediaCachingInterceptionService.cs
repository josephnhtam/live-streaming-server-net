using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpMediaCachingInterceptionService : IRtmpMediaCachingInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaCachingInterceptor> _interceptors;

        public RtmpMediaCachingInterceptionService(IEnumerable<IRtmpMediaCachingInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public async ValueTask CachePictureAsync(string streamPath, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp)
        {
            if (!_interceptors.Any())
                return;

            var rentedBuffer = payloadBuffer.ToRentedBuffer();

            try
            {
                foreach (var interceptor in _interceptors)
                    await interceptor.OnCachePicture(streamPath, mediaType, rentedBuffer, timestamp);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }

        public async ValueTask CacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(string streamPath)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnClearGroupOfPicturesCache(streamPath);
        }
    }
}
