using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpFCUnpublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName);

    [RtmpCommand("FCUnpublish")]
    internal class RtmpFCUnpublishCommandHandler : RtmpCommandHandler<RtmpFCUnpublishCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpFCUnpublishCommand command,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
