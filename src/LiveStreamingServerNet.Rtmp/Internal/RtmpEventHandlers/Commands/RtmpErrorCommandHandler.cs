using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpErrorCommand(double TransactionId, IDictionary<string, object> Properties, IDictionary<string, object> Information);

    [RtmpCommand("_error")]
    internal class RtmpErrorCommandHandler : RtmpCommandHandler<RtmpErrorCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpErrorCommand command,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
