using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPauseCommand(double TransactionId, IDictionary<string, object> CommandObject, bool Flag, double MilliSeconds);

    [RtmpCommand("pause")]
    internal class RtmpPauseCommandHandler : RtmpCommandHandler<RtmpPauseCommand>
    {
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;

        public RtmpPauseCommandHandler(
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageManagerService mediaMessageManager)
        {
            _userControlMessageSender = userControlMessageSender;
            _streamManager = streamManager;
            _mediaMessageManager = mediaMessageManager;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpPauseCommand command,
            CancellationToken cancellationToken)
        {
            var subscriptionContext = clientContext.StreamSubscriptionContext;

            if (subscriptionContext != null)
            {
                subscriptionContext.IsPaused = command.Flag;

                if (command.Flag)
                    HandlePause(clientContext);
                else
                    HandleUnpause(clientContext, chunkStreamContext);
            }

            return Task.FromResult(true);
        }

        private void HandleUnpause(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _userControlMessageSender.SendStreamBeginMessage(clientContext);
            SendCachedStreamMessages(clientContext, chunkStreamContext);
        }

        private void HandlePause(IRtmpClientContext clientContext)
        {
            _userControlMessageSender.SendStreamEofMessage(clientContext);
        }

        private void SendCachedStreamMessages(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            if (clientContext.StreamSubscriptionContext == null)
                return;

            var publishStreamContext = _streamManager.GetPublishStreamContext(clientContext.StreamSubscriptionContext.StreamPath);

            if (publishStreamContext == null)
                return;

            _mediaMessageManager.SendCachedStreamMetaDataMessage(
                clientContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);

            _mediaMessageManager.SendCachedHeaderMessages(
                clientContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);
        }
    }
}
