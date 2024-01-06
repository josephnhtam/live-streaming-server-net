using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpDeleteStreamCommand(double TransactionId, IDictionary<string, object> CommandObject, double StreamId);

    [RtmpCommand("deleteStream")]
    internal class RtmpDeleteStreamCommandHandler : RtmpCommandHandler<RtmpDeleteStreamCommand>
    {
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpDeleteStreamCommandHandler(IRtmpStreamDeletionService streamDeletionService)
        {
            _streamDeletionService = streamDeletionService;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpDeleteStreamCommand command,
            CancellationToken cancellationToken)
        {
            uint streamId = (uint)Math.Round(command.StreamId);

            if (streamId == peerContext.StreamId)
                _streamDeletionService.DeleteStream(peerContext);

            return Task.FromResult(true);
        }
    }
}
