using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpCloseCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("close")]
    internal class RtmpCloseCommandHandler : RtmpCommandHandler<RtmpCloseCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpCloseCommand command,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
