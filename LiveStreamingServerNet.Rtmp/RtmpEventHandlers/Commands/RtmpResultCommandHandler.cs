using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpResultCommand(double TransactionId, IDictionary<string, object> Properties, IDictionary<string, object> Information);

    [RtmpCommand("_result")]
    internal class RtmpResultCommandHandler : RtmpCommandHandler<RtmpResultCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpResultCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
