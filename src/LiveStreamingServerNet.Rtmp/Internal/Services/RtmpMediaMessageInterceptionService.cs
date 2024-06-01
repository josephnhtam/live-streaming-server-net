using LiveStreamingServerNet.Networking.Contracts;
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

        public async ValueTask CachePictureAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnCachePicture(streamPath, mediaType, rentedBuffer, timestamp);
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

        public async ValueTask ReceiveMediaMessageAsync(string streamPath, MediaType mediaType, INetBuffer payloadBuffer, uint timestamp, bool isSkippable)
        {
            if (!_interceptors.Any())
                return;

            var rentedBuffer = payloadBuffer.ToRentedBuffer();

            try
            {
                foreach (var interceptor in _interceptors)
                    await interceptor.OnReceiveMediaMessage(streamPath, mediaType, rentedBuffer, timestamp, isSkippable);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }
    }
}
