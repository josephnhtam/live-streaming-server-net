using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpMediaCachingInterceptor
    {
        bool FilterCache(uint clientId, string streamPath, MediaType mediaType) => true;
        ValueTask OnCacheSequenceHeaderAsync(uint clientId, string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask OnCachePictureAsync(uint clientId, string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask OnClearGroupOfPicturesCacheAsync(uint clientId, string streamPath);
    }
}
