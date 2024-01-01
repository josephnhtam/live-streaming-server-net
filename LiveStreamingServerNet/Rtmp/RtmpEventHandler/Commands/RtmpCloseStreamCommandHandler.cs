using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Commands
{
    public record RtmpCloseStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("closeStream")]
    public class RtmpCloseStreamCommandHandler : RtmpCommandHandler<RtmpCloseStreamCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpCloseStreamCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
