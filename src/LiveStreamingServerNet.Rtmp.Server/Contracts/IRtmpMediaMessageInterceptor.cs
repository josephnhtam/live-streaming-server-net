using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpMediaMessageInterceptor
    {
        bool FilterMediaMessage(uint clientId, string streamPath, MediaType mediaType, uint timestamp, bool isSkippable) => true;
        ValueTask OnReceiveMediaMessageAsync(uint clientId, string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
