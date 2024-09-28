using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpReceiveVideoCommand(double TransactionId, IDictionary<string, object> CommandObject, bool Flag);

    [RtmpCommand("receiveVideo")]
    internal class RtmpReceiveVideoCommandHandler : RtmpCommandHandler<RtmpReceiveVideoCommand, IRtmpClientSessionContext>
    {
        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpReceiveVideoCommand command,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var subscribeStreamContext = clientContext.GetStreamContext(streamId)?.SubscribeContext;

            if (subscribeStreamContext != null)
            {
                subscribeStreamContext.IsReceivingVideo = command.Flag;
            }

            return ValueTask.FromResult(true);
        }
    }
}
