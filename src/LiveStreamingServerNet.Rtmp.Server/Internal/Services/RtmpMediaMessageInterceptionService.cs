using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaMessageInterceptionService : IRtmpMediaMessageInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaMessageInterceptor> _interceptors;
        private readonly IPool<List<IRtmpMediaMessageInterceptor>> _interceptorListPool;

        public RtmpMediaMessageInterceptionService(IEnumerable<IRtmpMediaMessageInterceptor> interceptors)
        {
            _interceptors = interceptors;

            _interceptorListPool = new Pool<List<IRtmpMediaMessageInterceptor>>(
                () => new List<IRtmpMediaMessageInterceptor>(),
                obtainCallback: null,
                recycleCallback: x => x.Clear());
        }

        public async ValueTask ReceiveMediaMessageAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp, bool isSkippable)
        {
            var streamPath = publishStreamContext.StreamPath;
            var interceptors = GetFilteredInterceptors(streamPath, mediaType, timestamp, isSkippable);

            try
            {
                if (!interceptors.Any())
                    return;

                var rentedBuffer = payloadBuffer.ToRentedBuffer();

                try
                {
                    foreach (var interceptor in interceptors)
                        await interceptor.OnReceiveMediaMessageAsync(streamPath, mediaType, rentedBuffer, timestamp, isSkippable);
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

        private List<IRtmpMediaMessageInterceptor> GetFilteredInterceptors(string streamPath, MediaType mediaType, uint timestamp, bool isSkippable)
        {
            var interceptors = _interceptorListPool.Obtain();

            foreach (var interceptor in _interceptors)
            {
                if (interceptor.FilterMediaMessage(streamPath, mediaType, timestamp, isSkippable))
                    interceptors.Add(interceptor);
            }

            return interceptors;
        }
    }
}
