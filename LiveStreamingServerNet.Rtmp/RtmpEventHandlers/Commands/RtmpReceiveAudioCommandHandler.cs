using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    internal record RtmpReceiveAudioCommand(double TransactionId, IDictionary<string, object> CommandObject, bool Flag);

    [RtmpCommand("receiveAudio")]
    internal class RtmpReceiveAudioCommandHandler : RtmpCommandHandler<RtmpReceiveAudioCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpReceiveAudioCommand command,
            CancellationToken cancellationToken)
        {
            var subscriptionContext = peerContext.StreamSubscriptionContext;

            if (subscriptionContext != null)
            {
                subscriptionContext.IsReceivingAudio = command.Flag;
            }

            return Task.FromResult(true);
        }
    }
}
