using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts
{
    internal interface IRtmpMessageHandler
    {
        ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, INetBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}
