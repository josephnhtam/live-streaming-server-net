using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpOnStatusCommand(double TransactionId, IDictionary<string, object> Properties, IDictionary<string, object> Information);

    [RtmpCommand("onStatus")]
    internal class RtmpOnStatusCommandHandler : RtmpCommandHandler<RtmpOnStatusCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpOnStatusCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
