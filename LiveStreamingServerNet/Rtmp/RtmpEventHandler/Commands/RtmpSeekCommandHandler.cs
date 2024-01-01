using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Commands
{
    public record RtmpSeekCommand(double TransactionId, IDictionary<string, object> CommandObject, double MilliSeconds);

    [RtmpCommand("seek")]
    public class RtmpSeekCommandHandler : RtmpCommandHandler<RtmpSeekCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpSeekCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
