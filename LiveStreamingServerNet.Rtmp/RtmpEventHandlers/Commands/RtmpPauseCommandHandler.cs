using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
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
            IRtmpClientPeerContext peerContext,
            RtmpPauseCommand command,
            CancellationToken cancellationToken)
        {
            var subscriptionContext = peerContext.StreamSubscriptionContext;

            if (subscriptionContext != null)
            {
                subscriptionContext.IsPaused = command.Flag;

                if (command.Flag)
                    HandlePause(peerContext);
                else
                    HandleUnpause(peerContext, chunkStreamContext);
            }

            return Task.FromResult(true);
        }

        private void HandleUnpause(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _userControlMessageSender.SendStreamBeginMessage(peerContext);
            SendCachedStreamMessages(peerContext, chunkStreamContext);
        }

        private void HandlePause(IRtmpClientPeerContext peerContext)
        {
            _userControlMessageSender.SendStreamEofMessage(peerContext);
        }

        private void SendCachedStreamMessages(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            if (peerContext.StreamSubscriptionContext == null)
                return;

            var publishStreamContext = _streamManager.GetPublishStreamContext(peerContext.StreamSubscriptionContext.StreamPath);

            if (publishStreamContext == null)
                return;

            _mediaMessageManager.SendCachedStreamMetaDataMessage(
                peerContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);

            _mediaMessageManager.SendCachedHeaderMessages(
                peerContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);
        }
    }
}
