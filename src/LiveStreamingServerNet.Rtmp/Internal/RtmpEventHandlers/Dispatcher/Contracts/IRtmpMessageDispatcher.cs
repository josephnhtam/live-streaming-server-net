using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts
{
    internal interface IRtmpMessageDispatcher<TContext>
    {
        ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, CancellationToken cancellationToken);
    }
}
