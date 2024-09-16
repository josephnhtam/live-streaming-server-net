using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts
{
    internal interface IRtmpCommandDispatcher<TContext>
    {
        ValueTask<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, IDataBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}