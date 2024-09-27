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

            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var stream = clientContext.GetStream(streamId);

            if (stream == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.Id);
                return false;
            }

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(stream, command, chunkStreamContext, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            await StartSubscribingAsync(stream, command, chunkStreamContext, streamPath, streamArguments);
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
            IRtmpStream stream,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizeSubscribingAsync(stream.ClientContext, streamPath, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(stream.ClientContext.Client.Id, streamPath, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(stream, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartSubscribingAsync(
            IRtmpStream stream,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _streamManager.StartSubscribing(stream, streamPath, streamArguments);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(stream.ClientContext.Client.Id, streamPath);
                    SendSubscriptionStartedMessage(stream);
                    SendCachedStreamMessages(stream, chunkStreamContext);
                    await CompleteSubscriptionInitializationAsync(stream);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(stream.ClientContext.Client.Id, streamPath);
                    SendBadConnectionCommandMessage(stream, "Already subscribing.");
                    return false;

                case SubscribingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(stream.ClientContext.Client.Id, streamPath);
                    SendBadConnectionCommandMessage(stream, "Already publishing.");
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult), startSubscribingResult, null);
            }
        }

        private void SendCachedStreamMessages(IRtmpStream stream, IRtmpChunkStreamContext chunkStreamContext)
        {
            Debug.Assert(stream.SubscribeContext != null);

            var publishStreamContext = _streamManager.GetPublishStreamContext(stream.SubscribeContext.StreamPath);

            if (publishStreamContext == null)
                return;

            _mediaMessageCacher.SendCachedStreamMetaDataMessage(
                stream.SubscribeContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp);

            _mediaMessageCacher.SendCachedHeaderMessages(
                stream.SubscribeContext, publishStreamContext);

            if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                _mediaMessageCacher.SendCachedGroupOfPictures(
                    stream.SubscribeContext, publishStreamContext);
            }
        }

        private async ValueTask CompleteSubscriptionInitializationAsync(IRtmpStream stream)
        {
            Debug.Assert(stream.SubscribeContext != null);

            stream.SubscribeContext.CompleteInitialization();

            await _eventDispatcher.RtmpStreamSubscribedAsync(
                 stream.ClientContext,
                 stream.SubscribeContext.StreamPath,
                 stream.SubscribeContext.StreamArguments);
        }

        private async ValueTask SendAuthorizationFailedCommandMessageAsync(IRtmpStream stream, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                stream.ClientContext,
                stream.Id,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                reason);
        }

        private void SendSubscriptionStartedMessage(IRtmpStream stream)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                stream.ClientContext,
                stream.Id,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayStart,
                "Stream subscribed.");
        }

        private void SendBadConnectionCommandMessage(IRtmpStream stream, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                stream.ClientContext,
                stream.Id,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PlayBadConnection,
                reason);
        }
    }
}
