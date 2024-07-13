using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpMediaCachingInterceptor
    {
        ValueTask OnCacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask OnCachePictureAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask OnClearGroupOfPicturesCacheAsync(string streamPath);
    }
}
