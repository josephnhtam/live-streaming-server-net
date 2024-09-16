using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpDeleteStreamCommand(double TransactionId, IDictionary<string, object> CommandObject, double StreamId);

    [RtmpCommand("deleteStream")]
    internal class RtmpDeleteStreamCommandHandler : RtmpCommandHandler<RtmpDeleteStreamCommand, IRtmpClientSessionContext>
    {
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpDeleteStreamCommandHandler(IRtmpStreamDeletionService streamDeletionService)
        {
            _streamDeletionService = streamDeletionService;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpDeleteStreamCommand command,
            CancellationToken cancellationToken)
        {
            uint streamId = (uint)Math.Round(command.StreamId);

            if (streamId == clientContext.StreamId)
                await _streamDeletionService.DeleteStreamAsync(clientContext);

            return true;
        }
    }
}
