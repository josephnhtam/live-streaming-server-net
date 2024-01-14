using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
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

        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpDeleteStreamCommand command,
            CancellationToken cancellationToken)
        {
            uint streamId = (uint)Math.Round(command.StreamId);

            if (streamId == clientContext.StreamId)
                _streamDeletionService.DeleteStream(clientContext);

            return ValueTask.FromResult(true);
        }
    }
}
