using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaCachingInterceptionService
    {
        ValueTask CacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask CachePictureAsync(string streamPath, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp);
        ValueTask ClearGroupOfPicturesCacheAsync(string streamPath);
    }
}
