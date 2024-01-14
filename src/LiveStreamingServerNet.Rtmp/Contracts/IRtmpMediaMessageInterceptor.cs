using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpMediaMessageInterceptor
    {
        ValueTask OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask OnClearGroupOfPicturesCache(string streamPath);
        ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
