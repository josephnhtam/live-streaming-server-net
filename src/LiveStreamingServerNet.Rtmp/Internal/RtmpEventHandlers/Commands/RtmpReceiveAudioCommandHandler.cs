using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpReceiveAudioCommand(double TransactionId, IDictionary<string, object> CommandObject, bool Flag);

    [RtmpCommand("receiveAudio")]
    internal class RtmpReceiveAudioCommandHandler : RtmpCommandHandler<RtmpReceiveAudioCommand>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpReceiveAudioCommand command,
            CancellationToken cancellationToken)
        {
            var subscriptionContext = clientContext.StreamSubscriptionContext;

            if (subscriptionContext != null)
            {
                subscriptionContext.IsReceivingAudio = command.Flag;
            }

            return ValueTask.FromResult(true);
        }
    }
}
