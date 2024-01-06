using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    public record RtmpCloseStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("closeStream")]
    public class RtmpCloseStreamCommandHandler : RtmpCommandHandler<RtmpCloseStreamCommand>
    {
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpCloseStreamCommandHandler(IRtmpStreamDeletionService streamDeletionService)
        {
            _streamDeletionService = streamDeletionService;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpCloseStreamCommand command,
            CancellationToken cancellationToken)
        {
            if (peerContext.StreamId.HasValue)
                _streamDeletionService.DeleteStream(peerContext);

            return Task.FromResult(true);
        }
    }
}
