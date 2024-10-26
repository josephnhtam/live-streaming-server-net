using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaCachingInterceptionService : IRtmpMediaCachingInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaCachingInterceptor> _interceptors;
        private readonly IPool<List<IRtmpMediaCachingInterceptor>> _interceptorListPool;

        public RtmpMediaCachingInterceptionService(IEnumerable<IRtmpMediaCachingInterceptor> interceptors)
        {
            _interceptors = interceptors;

            _interceptorListPool = new Pool<List<IRtmpMediaCachingInterceptor>>(
                () => new List<IRtmpMediaCachingInterceptor>(),
                obtainCallback: null,
                recycleCallback: x => x.Clear());
        }

        public async ValueTask CachePictureAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp)
        {
            var clientId = publishStreamContext.StreamContext?.ClientContext.Client.Id ?? 0;
            var streamPath = publishStreamContext.StreamPath;
            var interceptors = GetFilteredInterceptors(clientId, streamPath, mediaType);

            try
            {
                if (!interceptors.Any())
                    return;

                var rentedBuffer = payloadBuffer.ToRentedBuffer();

                try
                {
                    foreach (var interceptor in _interceptors)
                        await interceptor.OnCachePictureAsync(clientId, streamPath, mediaType, rentedBuffer, timestamp);
                }
                finally
                {
                    rentedBuffer.Unclaim();
                }
            }
            finally
            {
                _interceptorListPool.Recycle(interceptors);
            }
        }

        public async ValueTask CacheSequenceHeaderAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, byte[] sequenceHeader)
        {
            var clientId = publishStreamContext.StreamContext?.ClientContext.Client.Id ?? 0;
            var streamPath = publishStreamContext.StreamPath;

            foreach (var interceptor in _interceptors)
            {
                if (interceptor.FilterCache(clientId, streamPath, mediaType))
                {
                    await interceptor.OnCacheSequenceHeaderAsync(clientId, streamPath, mediaType, sequenceHeader);
                }
            }
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            var clientId = publishStreamContext.StreamContext?.ClientContext.Client.Id ?? 0;
            var streamPath = publishStreamContext.StreamPath;

            foreach (var interceptor in _interceptors)
                await interceptor.OnClearGroupOfPicturesCacheAsync(clientId, streamPath);
        }

        private List<IRtmpMediaCachingInterceptor> GetFilteredInterceptors(uint clientId, string streamPath, MediaType mediaType)
        {
            var interceptors = _interceptorListPool.Obtain();

            foreach (var interceptor in _interceptors)
            {
                if (interceptor.FilterCache(clientId, streamPath, mediaType))
                    interceptors.Add(interceptor);
            }

            return interceptors;
        }
    }
}
