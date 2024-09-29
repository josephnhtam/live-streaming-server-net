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
    internal record RtmpPublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName, string PublishingType);

    [RtmpCommand("publish")]
    internal class RtmpPublishCommandHandler : RtmpCommandHandler<RtmpPublishCommand, IRtmpClientSessionContext>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly ILogger _logger;

        public RtmpPublishCommandHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            IStreamAuthorization streamAuthorization,
            ILogger<RtmpPublishCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _eventDispatcher = eventDispatcher;
            _streamAuthorization = streamAuthorization;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpPublishCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Publish(clientContext.Client.Id, command.PublishingName, command.PublishingType);

            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var streamContext = clientContext.GetStreamContext(streamId);

            if (streamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.Id);
                return false;
            }

            var (streamPath, streamArguments) = ParsePublishContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(streamContext, command, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            await StartPublishingAsync(streamContext, command, streamPath, streamArguments);
            return true;
        }

        private static (string StreamPath, IReadOnlyDictionary<string, string> StreamArguments)
            ParsePublishContext(RtmpPublishCommand command, IRtmpClientSessionContext clientContext)
        {
            Debug.Assert(!string.IsNullOrEmpty(clientContext.AppName));

            var (streamName, arguments) = StreamUtilities.ParseStreamName(command.PublishingName);
            var streamPath = StreamUtilities.ComposeStreamPath(clientContext.AppName, streamName);
            return (streamPath, arguments.AsReadOnly());
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpStreamContext streamContext,
            RtmpPublishCommand command,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizePublishingAsync(
                streamContext.ClientContext, streamPath, command.PublishingType, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(streamContext.ClientContext.Client.Id, streamPath, command.PublishingType, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(streamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartPublishingAsync(
            IRtmpStreamContext streamContext,
            RtmpPublishCommand command,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startPublishingResult = _streamManager.StartPublishing(
                streamContext, streamPath, streamArguments, out var subscribeStreamContexts);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.PublishingStarted(streamContext.ClientContext.Client.Id, streamPath, command.PublishingType);
                    SendPublishingStartedMessage(streamContext, subscribeStreamContexts);
                    await _eventDispatcher.RtmpStreamPublishedAsync(streamContext.ClientContext, streamPath, streamArguments);
                    return true;

                case PublishingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(streamContext.ClientContext.Client.Id, streamPath);
                    SendBadConnectionCommandMessage(streamContext, "Already subscribing.");
                    return false;

                case PublishingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(streamContext.ClientContext.Client.Id, streamPath);
                    SendBadConnectionCommandMessage(streamContext, "Already publishing.");
                    return false;

                case PublishingStreamResult.AlreadyExists:
                    _logger.StreamAlreadyExists(streamContext.ClientContext.Client.Id, streamPath, command.PublishingType);
                    SendAlreadyExistsCommandMessage(streamContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startPublishingResult), startPublishingResult, null);
            }
        }

        private void SendAlreadyExistsCommandMessage(IRtmpStreamContext streamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                streamContext.ClientContext,
                streamContext.StreamId,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private void SendBadConnectionCommandMessage(IRtmpStreamContext streamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                streamContext.ClientContext,
                streamContext.StreamId,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PublishBadConnection,
                reason);
        }

        private async ValueTask SendAuthorizationFailedCommandMessageAsync(IRtmpStreamContext streamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                streamContext.ClientContext,
                streamContext.StreamId,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PublishUnauthorized,
                reason);
        }

        private void SendPublishingStartedMessage(
            IRtmpStreamContext streamContext, IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                streamContext.ClientContext,
                streamContext.StreamId,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PublishStart,
                "Publishing started.");

            foreach (var batch in subscribeStreamContexts.GroupBy(x => x.StreamContext.StreamId))
            {
                var streamId = batch.Key;
                var subscribers = batch.Select(x => x.StreamContext.ClientContext).ToList();

                _commandMessageSender.SendOnStatusCommandMessage(
                   subscribers,
                   streamId,
                   RtmpStatusLevels.Status,
                   RtmpStreamStatusCodes.PlayReset,
                   "Stream started.");

                _commandMessageSender.SendOnStatusCommandMessage(
                    subscribers,
                    streamId,
                    RtmpStatusLevels.Status,
                    RtmpStreamStatusCodes.PlayStart,
                    "Stream started.");
            }
        }
    }
}
