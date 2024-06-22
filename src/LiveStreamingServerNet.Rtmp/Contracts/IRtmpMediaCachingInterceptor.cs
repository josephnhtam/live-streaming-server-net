using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpMediaCachingInterceptor
    {
        ValueTask OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask OnClearGroupOfPicturesCache(string streamPath);
    }
}
