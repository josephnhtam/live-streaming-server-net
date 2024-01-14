using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpOnFCPublishCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Information);

    [RtmpCommand("onFCPublish")]
    internal class RtmpOnFCPublishCommandHandler : RtmpCommandHandler<RtmpOnFCPublishCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpOnFCPublishCommand command,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }
    }
}
