using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher
{
    internal abstract class RtmpCommandHandler<TContext>
    {
        public abstract ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, object command, CancellationToken cancellationToken);
    }

    internal abstract class RtmpCommandHandler<TCommand, TContext> : RtmpCommandHandler<TContext>
    {
        public sealed override ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, object command, CancellationToken cancellationToken)
        {
            return HandleAsync(chunkStreamContext, context, (TCommand)command, cancellationToken);
        }

        public abstract ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, TContext context, TCommand command, CancellationToken cancellationToken);
    }
}
