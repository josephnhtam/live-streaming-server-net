using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpFCUnpublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName);

    [RtmpCommand("FCUnpublish")]
    internal class RtmpFCUnpublishCommandHandler : RtmpCommandHandler<RtmpFCUnpublishCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpFCUnpublishCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
