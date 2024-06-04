using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpCloseStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("closeStream")]
    internal class RtmpCloseStreamCommandHandler : RtmpCommandHandler<RtmpCloseStreamCommand>
    {
        private readonly IRtmpStreamDeletionService _streamDeletionService;

        public RtmpCloseStreamCommandHandler(IRtmpStreamDeletionService streamDeletionService)
        {
            _streamDeletionService = streamDeletionService;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpCloseStreamCommand command,
            CancellationToken cancellationToken)
        {
            if (clientContext.StreamId.HasValue)
                await _streamDeletionService.DeleteStreamAsync(clientContext);

            return true;
        }
    }
}
