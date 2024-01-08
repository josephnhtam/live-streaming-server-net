using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpMediaMessageInterceptor
    {
        Task OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        Task OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        Task OnClearGroupOfPicturesCache(string streamPath);
        Task OnEnqueueMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
