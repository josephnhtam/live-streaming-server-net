using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Auth;
using LiveStreamingServerNet.Rtmp.Server.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Server.Internal.Utilities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPlayCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName, double Start, double Duration, bool Reset);

    [RtmpCommand("play")]
    internal class RtmpPlayCommandHandler : RtmpCommandHandler<RtmpPlayCommand, IRtmpClientSessionContext>
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
            IRtmpClientSessionContext clientContext,
            RtmpPlayCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Play(clientContext.Client.Id, command.StreamName);

            if (clientContext.StreamId == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.Id);
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
            ParseSubscriptionContext(RtmpPlayCommand command, IRtmpClientSessionContext clientContext)
        {
            Debug.Assert(!string.IsNullOrEmpty(clientContext.AppName));

            var (streamName, arguments) = StreamUtilities.ParseStreamName(command.StreamName);
            var streamPath = StreamUtilities.ComposeStreamPath(clientContext.AppName, streamName);
            return (streamPath, arguments.AsReadOnly());
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientSessionContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizeSubscribingAsync(clientContext, streamPath, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(clientContext.Client.Id, streamPath, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(clientContext, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartSubscribingAsync(
            IRtmpClientSessionContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _streamManager.StartSubscribingStream(clientContext, chunkStreamContext.ChunkStreamId, streamPath, streamArguments);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(clientContext.Client.Id, streamPath);
                    SendSubscriptionStartedMessage(clientContext, chunkStreamContext);
                    SendCachedStreamMessages(clientContext, chunkStreamContext);
                    await CompleteSubscriptionInitializationAsync(clientContext);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(clientContext.Client.Id, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already subscribing.");
                    return false;

                case SubscribingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(clientContext.Client.Id, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already publishing.");
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult), startSubscribingResult, null);
            }
        }

        private void SendCachedStreamMessages(IRtmpClientSessionContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
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

        private async ValueTask CompleteSubscriptionInitializationAsync(IRtmpClientSessionContext clientContext)
        {
            clientContext.StreamSubscriptionContext!.CompleteInitialization();

            await _eventDispatcher.RtmpStreamSubscribedAsync(
                 clientContext,
                 clientContext.StreamSubscriptionContext.StreamPath,
                 clientContext.StreamSubscriptionContext.StreamArguments);
        }

        private async ValueTask SendAuthorizationFailedCommandMessageAsync(IRtmpClientSessionContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                clientContext.StreamId ?? 0,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                reason);
        }

        private void SendSubscriptionStartedMessage(IRtmpClientSessionContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                clientContext.StreamId ?? 0,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayStart,
                "Stream subscribed.");
        }

        private void SendBadConnectionCommandMessage(IRtmpClientSessionContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                clientContext.StreamId ?? 0,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PlayBadConnection,
                reason);
        }
    }
}
