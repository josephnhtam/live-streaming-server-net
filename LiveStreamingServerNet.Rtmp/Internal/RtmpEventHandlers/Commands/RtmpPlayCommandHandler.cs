using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPlayCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName, double Start, double Duration, bool Reset);

    [RtmpCommand("play")]
    internal class RtmpPlayCommandHandler : RtmpCommandHandler<RtmpPlayCommand>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatch;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;

        public RtmpPlayCommandHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpMediaMessageManagerService mediaMessageManager,
            IRtmpServerStreamEventDispatcher eventDispatch,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpPlayCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _mediaMessageManager = mediaMessageManager;
            _eventDispatch = eventDispatch;
            _config = config.Value;
            _logger = logger;
        }

        public override async Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Play(clientContext.Client.ClientId, command.StreamName);

            if (clientContext.StreamId == null)
                throw new InvalidOperationException("Stream is not yet created.");

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, clientContext);

            if (await AuthorizeAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments))
            {
                StartSubscribing(clientContext, command, chunkStreamContext, streamPath, streamArguments);
            }

            return true;
        }

        private Task<bool> AuthorizeAsync(IRtmpClientContext clientContext, string streamPath, IDictionary<string, string> streamArguments)
        {
            return Task.FromResult(true);
        }

        private static (string StreamPath, IDictionary<string, string> StreamArguments)
            ParseSubscriptionContext(RtmpPlayCommand command, IRtmpClientContext clientContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamPath(command.StreamName);

            var streamPath = $"/{string.Join('/',
                new string[] { clientContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            return (streamPath, arguments);
        }

        private async Task<bool> AuthorizeAsync(
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            if (!await AuthorizeAsync(clientContext, streamPath, streamArguments))
            {
                _logger.AuthorizationFailed(clientContext.Client.ClientId, streamPath);
                SendAuthorizationFailedCommandMessage(clientContext, chunkStreamContext);
                return false;
            }

            return true;
        }

        private bool StartSubscribing(
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _streamManager.StartSubscribingStream(clientContext, chunkStreamContext.ChunkStreamId, streamPath, streamArguments);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(clientContext.Client.ClientId, streamPath);
                    SendSubscriptionStartedMessage(clientContext, chunkStreamContext);
                    SendCachedStreamMessages(clientContext, chunkStreamContext);
                    CompleteSubscriptionInitialization(clientContext);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already subscribing.");
                    return false;

                case SubscribingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already publishing.");
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult), startSubscribingResult, null);
            }
        }

        private void SendCachedStreamMessages(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            var publishStreamContext = _streamManager.GetPublishStreamContext(clientContext.StreamSubscriptionContext!.StreamPath);

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

            if (_config.EnableGopCaching)
            {
                _mediaMessageManager.SendCachedGroupOfPictures(
                    clientContext, publishStreamContext,
                    chunkStreamContext.MessageHeader.MessageStreamId);
            }
        }

        private void CompleteSubscriptionInitialization(IRtmpClientContext clientContext)
        {
            clientContext.StreamSubscriptionContext!.CompleteInitialization();

            _eventDispatch.RtmpStreamSubscribedAsync(
                clientContext,
                clientContext.StreamSubscriptionContext.StreamPath,
                clientContext.StreamSubscriptionContext.StreamArguments.AsReadOnly());
        }

        private void SendAuthorizationFailedCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                "Authorization failed.");
        }

        private void SendSubscriptionStartedMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayStart,
                "Stream subscribed.");
        }

        private void SendBadConnectionCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PlayBadConnection,
                reason);
        }
    }
}
