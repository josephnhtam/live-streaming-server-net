﻿using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher
{
    internal abstract class RtmpCommandHandler
    {
        public abstract Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, object command, CancellationToken cancellationToken);
    }

    internal abstract class RtmpCommandHandler<TCommand> : RtmpCommandHandler
    {
        public sealed override Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, object command, CancellationToken cancellationToken)
        {
            return HandleAsync(chunkStreamContext, clientContext, (TCommand)command, cancellationToken);
        }

        public abstract Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientContext clientContext, TCommand command, CancellationToken cancellationToken);
    }
}
