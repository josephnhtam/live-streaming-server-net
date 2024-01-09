using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageInterceptionService
    {
        Task CacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        Task CachePictureAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        Task ClearGroupOfPicturesCacheAsync(string streamPath);
        Task ReceiveMediaMessageAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
