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
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;

        public RtmpPlayCommandHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            IStreamAuthorization streamAuthorization,
            ILogger<RtmpPlayCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _userControlMessageSender = userControlMessageSender;
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
            var streamContext = clientContext.GetStreamContext(streamId);

            if (streamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.Id);
                return false;
            }

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(streamContext, command, chunkStreamContext, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            await StartSubscribingAsync(streamContext, command, chunkStreamContext, streamPath, streamArguments);
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
            IRtmpStreamContext streamContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizeSubscribingAsync(streamContext.ClientContext, streamPath, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(streamContext.ClientContext.Client.Id, streamPath, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(streamContext, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartSubscribingAsync(
            IRtmpStreamContext streamContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _streamManager.StartSubscribing(streamContext, streamPath, streamArguments, out var publishStreamContext);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(streamContext.ClientContext.Client.Id, streamPath);
                    SendCachedStreamMessages(streamContext, chunkStreamContext, publishStreamContext);
                    await CompleteSubscriptionInitializationAsync(streamContext);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(streamContext.ClientContext.Client.Id, streamPath);
                    return false;

                case SubscribingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(streamContext.ClientContext.Client.Id, streamPath);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult), startSubscribingResult, null);
            }
        }

        private void SendCachedStreamMessages(
            IRtmpStreamContext streamContext, IRtmpChunkStreamContext chunkStreamContext, IRtmpPublishStreamContext? publishStreamContext)
        {
            Debug.Assert(streamContext.SubscribeContext != null);

            if (publishStreamContext == null)
                return;

            _mediaMessageCacher.SendCachedStreamMetaDataMessage(
                streamContext.SubscribeContext, publishStreamContext,
                chunkStreamContext.Timestamp);

            _mediaMessageCacher.SendCachedHeaderMessages(
                streamContext.SubscribeContext, publishStreamContext);

            if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                _mediaMessageCacher.SendCachedGroupOfPictures(
                    streamContext.SubscribeContext, publishStreamContext);
            }
        }

        private async ValueTask CompleteSubscriptionInitializationAsync(IRtmpStreamContext streamContext)
        {
            Debug.Assert(streamContext.SubscribeContext != null);

            streamContext.SubscribeContext.CompleteInitialization();

            await _eventDispatcher.RtmpStreamSubscribedAsync(
                 streamContext.ClientContext,
                 streamContext.SubscribeContext.StreamPath,
                 streamContext.SubscribeContext.StreamArguments);
        }

        private async ValueTask SendAuthorizationFailedCommandMessageAsync(
            IRtmpStreamContext streamContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                streamContext,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PlayFailed,
                reason);
        }
    }
}
