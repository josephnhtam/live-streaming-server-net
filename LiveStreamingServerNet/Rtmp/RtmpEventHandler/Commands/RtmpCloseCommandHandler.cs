using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Commands
{
    public record RtmpCloseCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("close")]
    public class RtmpCloseCommandHandler : RtmpCommandHandler<RtmpCloseCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpCloseCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
