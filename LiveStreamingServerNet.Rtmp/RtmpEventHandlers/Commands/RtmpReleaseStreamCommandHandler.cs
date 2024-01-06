using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpReleaseStreamCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName);

    [RtmpCommand("releaseStream")]
    internal class RtmpReleaseStreamCommandHandler : RtmpCommandHandler<RtmpReleaseStreamCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpReleaseStreamCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
