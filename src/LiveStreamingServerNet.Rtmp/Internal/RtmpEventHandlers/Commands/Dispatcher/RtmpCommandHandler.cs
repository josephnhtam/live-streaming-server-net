using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher
{
    internal abstract class RtmpCommandHandler
    {
        public abstract ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, object command, CancellationToken cancellationToken);
    }

    internal abstract class RtmpCommandHandler<TCommand> : RtmpCommandHandler
    {
        public sealed override ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, object command, CancellationToken cancellationToken)
        {
            return HandleAsync(chunkStreamContext, clientContext, (TCommand)command, cancellationToken);
        }

        public abstract ValueTask<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, TCommand command, CancellationToken cancellationToken);
    }
}
