using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpReceiveVideoCommand(double TransactionId, IDictionary<string, object> CommandObject, bool Flag);

    [RtmpCommand("receiveVideo")]
    internal class RtmpReceiveVideoCommandHandler : RtmpCommandHandler<RtmpReceiveVideoCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpReceiveVideoCommand command,
            CancellationToken cancellationToken)
        {
            var subscriptionContext = clientContext.StreamSubscriptionContext;

            if (subscriptionContext != null)
            {
                subscriptionContext.IsReceivingVideo = command.Flag;
            }

            return ValueTask.FromResult(true);
        }
    }
}
