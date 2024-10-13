using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpMediaCachingInterceptor
    {
        bool FilterCache(string streamPath, MediaType mediaType) => true;
        ValueTask OnCacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask OnCachePictureAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask OnClearGroupOfPicturesCacheAsync(string streamPath);
    }
}
