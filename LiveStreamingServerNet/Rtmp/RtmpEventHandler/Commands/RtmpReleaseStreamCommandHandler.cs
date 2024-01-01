using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Commands
{
    public record RtmpReleaseStreamCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName);

    [RtmpCommand("releaseStream")]
    public class RtmpReleaseStreamCommandHandler : RtmpCommandHandler<RtmpReleaseStreamCommand>
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
