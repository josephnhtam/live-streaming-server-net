using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts
{
    internal interface IRtmpMessageHandler<TContext>
    {
        ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, IDataBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}
