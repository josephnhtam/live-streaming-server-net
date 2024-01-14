using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Contracts
{
    internal interface IRtmpMessageDispatcher
    {
        ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, CancellationToken cancellationToken);
    }
}
