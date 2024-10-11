using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaMessageInterceptionService : IRtmpMediaMessageInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaMessageInterceptor> _interceptors;

        public RtmpMediaMessageInterceptionService(IEnumerable<IRtmpMediaMessageInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public async ValueTask ReceiveMediaMessageAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp, bool isSkippable)
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
                    await interceptor.OnReceiveMediaMessageAsync(streamPath, mediaType, rentedBuffer, timestamp, isSkippable);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }
    }
}
