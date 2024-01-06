using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpSeekCommand(double TransactionId, IDictionary<string, object> CommandObject, double MilliSeconds);

    [RtmpCommand("seek")]
    internal class RtmpSeekCommandHandler : RtmpCommandHandler<RtmpSeekCommand>
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
