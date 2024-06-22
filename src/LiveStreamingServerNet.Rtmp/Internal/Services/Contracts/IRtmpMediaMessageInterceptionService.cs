using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageInterceptionService
    {
        ValueTask ReceiveMediaMessageAsync(string streamPath, MediaType mediaType, INetBuffer payloadBuffer, uint timestamp, bool isSkippable);
    }
}
