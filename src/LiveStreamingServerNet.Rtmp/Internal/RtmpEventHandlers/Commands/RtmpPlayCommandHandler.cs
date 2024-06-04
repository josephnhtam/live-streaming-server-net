using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPlayCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName, double Start, double Duration, bool Reset);

    [RtmpCommand("play")]
    internal class RtmpPlayCommandHandler : RtmpCommandHandler<RtmpPlayCommand>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;

        public RtmpPlayCommandHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            IStreamAuthorization streamAuthorization,
            ILogger<RtmpPlayCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _mediaMessageCacher = mediaMessageCacher;
            _eventDispatcher = eventDispatcher;
            _streamAuthorization = streamAuthorization;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Play(clientContext.Client.ClientId, command.StreamName);

            if (clientContext.StreamId == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.ClientId);
                return false;
            }

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            await StartSubscribingAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments);
            return true;
        }

        private static (string StreamPath, IReadOnlyDictionary<string, string> StreamArguments)
            ParseSubscriptionContext(RtmpPlayCommand command, IRtmpClientContext clientContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamName(command.StreamName);
            var streamPath = StreamUtilities.ComposeStreamPath(clientContext.AppName, streamName);
            return (streamPath, arguments.AsReadOnly());
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizeSubscribingAsync(clientContext, streamPath, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(clientContext.Client.ClientId, streamPath, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(clientContext, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartSubscribingAsync(
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _streamManager.StartSubscribingStream(clientContext, chunkStreamContext.ChunkStreamId, streamPath, streamArguments);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(clientContext.Client.ClientId, streamPath);
                    SendSubscriptionStartedMessage(clientContext, chunkStreamContext);
                    SendCachedStreamMessages(clientContext, chunkStreamContext);
                    await CompleteSubscriptionInitializationAsync(clientContext);
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

            _mediaMessageCacher.SendCachedStreamMetaDataMessage(
                clientContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);

            _mediaMessageCacher.SendCachedHeaderMessages(
                clientContext, publishStreamContext,
                chunkStreamContext.MessageHeader.MessageStreamId);

            if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                _mediaMessageCacher.SendCachedGroupOfPictures(
                    clientContext, publishStreamContext,
                    chunkStreamContext.MessageHeader.MessageStreamId);
            }
        }

        private async ValueTask CompleteSubscriptionInitializationAsync(IRtmpClientContext clientContext)
        {
            clientContext.StreamSubscriptionContext!.CompleteInitialization();

            await _eventDispatcher.RtmpStreamSubscribedAsync(
                 clientContext,
                 clientContext.StreamSubscriptionContext.StreamPath,
                 clientContext.StreamSubscriptionContext.StreamArguments);
        }

        private async ValueTask SendAuthorizationFailedCommandMessageAsync(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                reason);
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
