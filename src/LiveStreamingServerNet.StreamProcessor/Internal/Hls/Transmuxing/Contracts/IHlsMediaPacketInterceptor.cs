using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts
{
    internal interface IHlsMediaPacketInterceptor
    {
        ValueTask InterceptMediaPacketAsync(MediaType mediaType, IRentedBuffer buffer, uint timestamp);
    }
}
