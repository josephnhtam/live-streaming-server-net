using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpSeekCommand(double TransactionId, IDictionary<string, object> CommandObject, double MilliSeconds);

    [RtmpCommand("seek")]
    internal class RtmpSeekCommandHandler : RtmpCommandHandler<RtmpSeekCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpSeekCommand command,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
