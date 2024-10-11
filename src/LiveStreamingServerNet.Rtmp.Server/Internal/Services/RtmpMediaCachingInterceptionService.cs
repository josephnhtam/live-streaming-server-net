using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaCachingInterceptionService : IRtmpMediaCachingInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaCachingInterceptor> _interceptors;

        public RtmpMediaCachingInterceptionService(IEnumerable<IRtmpMediaCachingInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public async ValueTask CachePictureAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp)
        {
            if (publishStreamContext.StreamContext == null)
                return;

            if (!_interceptors.Any())
                return;

            var rentedBuffer = payloadBuffer.ToRentedBuffer();

            try
            {
                var streamPath = publishStreamContext.StreamPath;

                foreach (var interceptor in _interceptors)
                    await interceptor.OnCachePictureAsync(streamPath, mediaType, rentedBuffer, timestamp);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }

        public async ValueTask CacheSequenceHeaderAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, byte[] sequenceHeader)
        {
            if (publishStreamContext.StreamContext == null)
                return;

            var streamPath = publishStreamContext.StreamPath;

            foreach (var interceptor in _interceptors)
                await interceptor.OnCacheSequenceHeaderAsync(streamPath, mediaType, sequenceHeader);
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            if (publishStreamContext.StreamContext == null)
                return;

            var streamPath = publishStreamContext.StreamPath;

            foreach (var interceptor in _interceptors)
                await interceptor.OnClearGroupOfPicturesCacheAsync(streamPath);
        }
    }
}
