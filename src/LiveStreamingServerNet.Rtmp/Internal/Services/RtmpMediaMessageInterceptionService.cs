using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpMediaMessageInterceptionService : IRtmpMediaMessageInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaMessageInterceptor> _interceptors;

        public RtmpMediaMessageInterceptionService(IEnumerable<IRtmpMediaMessageInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public async ValueTask ReceiveMediaMessageAsync(string streamPath, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp, bool isSkippable)
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
