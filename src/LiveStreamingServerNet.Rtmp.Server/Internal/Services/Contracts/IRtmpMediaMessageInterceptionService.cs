using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageInterceptionService
    {
        ValueTask ReceiveMediaMessageAsync(string streamPath, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp, bool isSkippable);
    }
}
