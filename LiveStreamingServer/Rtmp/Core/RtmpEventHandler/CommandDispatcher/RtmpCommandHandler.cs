using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher
{
    public abstract class RtmpCommandHandler
    {
        public abstract Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, object command, CancellationToken cancellationToken);
    }

    public abstract class RtmpCommandHandler<TCommand> : RtmpCommandHandler
    {
        public sealed override Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, object command, CancellationToken cancellationToken)
        {
            return HandleAsync(chunkStreamContext, peerContext, (TCommand)command, cancellationToken);
        }

        public abstract Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, TCommand command, CancellationToken cancellationToken);
    }
}
