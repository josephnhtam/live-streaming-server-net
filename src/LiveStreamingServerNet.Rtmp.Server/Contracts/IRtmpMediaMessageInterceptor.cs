using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpMediaMessageInterceptor
    {
        bool FilterMediaMessage(string streamPath, MediaType mediaType, uint timestamp, bool isSkippable) => true;
        ValueTask OnReceiveMediaMessageAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
