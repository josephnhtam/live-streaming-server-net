using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpReceiveAudioCommand(double TransactionId, IDictionary<string, object> CommandObject, bool Flag);

    [RtmpCommand("receiveAudio")]
    internal class RtmpReceiveAudioCommandHandler : RtmpCommandHandler<RtmpReceiveAudioCommand, IRtmpClientSessionContext>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpReceiveAudioCommand command,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var subscribeStreamContext = clientContext.GetStreamContext(streamId)?.SubscribeContext;

            if (subscribeStreamContext != null)
            {
                subscribeStreamContext.IsReceivingAudio = command.Flag;
            }

            return ValueTask.FromResult(true);
        }
    }
}
