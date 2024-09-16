using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpMediaMessageInterceptor
    {
        ValueTask OnReceiveMediaMessageAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
