using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageInterceptionService
    {
        ValueTask CacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader);
        ValueTask CachePictureAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
        ValueTask ClearGroupOfPicturesCacheAsync(string streamPath);
        ValueTask ReceiveMediaMessageAsync(string streamPath, MediaType mediaType, INetBuffer payloadBuffer, uint timestamp, bool isSkippable);
    }
}
