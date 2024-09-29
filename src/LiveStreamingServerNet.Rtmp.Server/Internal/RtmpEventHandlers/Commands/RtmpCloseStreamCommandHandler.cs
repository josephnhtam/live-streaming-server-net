using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpCloseStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("closeStream")]
    internal class RtmpCloseStreamCommandHandler : RtmpCommandHandler<RtmpCloseStreamCommand, IRtmpClientSessionContext>
    {
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpCloseStreamCommandHandler(IRtmpStreamDeletionService streamDeletionService)
        {
            _streamDeletionService = streamDeletionService;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpCloseStreamCommand command,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var streamContext = clientContext.GetStreamContext(streamId);

            if (streamContext != null)
                await _streamDeletionService.CloseStreamAsync(streamContext);

            return true;
        }
    }
}
