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
    internal record RtmpPublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName, string PublishingType);

    [RtmpCommand("publish")]
    internal class RtmpPublishCommandHandler : RtmpCommandHandler<RtmpPublishCommand>
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
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Publish(clientContext.Client.ClientId, command.PublishingName, command.PublishingType);

            if (clientContext.StreamId == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.ClientId);
                return false;
            }

            var (streamPath, streamArguments) = ParsePublishContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            await StartPublishingAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments);
            return true;
        }

        private static (string StreamPath, IReadOnlyDictionary<string, string> StreamArguments)
            ParsePublishContext(RtmpPublishCommand command, IRtmpClientContext clientContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamName(command.PublishingName);
            var streamPath = StreamUtilities.ComposeStreamPath(clientContext.AppName, streamName);
            return (streamPath, arguments.AsReadOnly());
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizePublishingAsync(
                clientContext, streamPath, command.PublishingType, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(clientContext.Client.ClientId, streamPath, command.PublishingType, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(clientContext, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartPublishingAsync(
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startPublishingResult = _streamManager.StartPublishingStream(clientContext, streamPath, streamArguments, out _);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.PublishingStarted(clientContext.Client.ClientId, streamPath, command.PublishingType);
                    SendPublishingStartedMessage(clientContext, chunkStreamContext);
                    await _eventDispatcher.RtmpStreamPublishedAsync(clientContext, streamPath, streamArguments);
                    return true;

                case PublishingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already subscribing.");
                    return false;

                case PublishingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already publishing.");
                    return false;

                case PublishingStreamResult.AlreadyExists:
                    _logger.StreamAlreadyExists(clientContext.Client.ClientId, streamPath, command.PublishingType);
                    SendAlreadyExistsCommandMessage(clientContext, chunkStreamContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startPublishingResult), startPublishingResult, null);
            }
        }

        private void SendAlreadyExistsCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private void SendBadConnectionCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadConnection,
                reason);
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

        private void SendPublishingStartedMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PublishStart,
                "Publishing started.");
        }
    }
}
