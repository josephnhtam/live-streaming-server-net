using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpResultCommand(double TransactionId, IDictionary<string, object> Properties, IDictionary<string, object> Information);

    [RtmpCommand("_result")]
    internal class RtmpResultCommandHandler : RtmpCommandHandler<RtmpResultCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpResultCommand command,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
