using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Logging;
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
        private readonly ILogger _logger;

        public RtmpPublishCommandHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            ILogger<RtmpPublishCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public override async Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Publish(clientContext.Client.ClientId, command.PublishingName, command.PublishingType);

            if (clientContext.StreamId == null)
                throw new InvalidOperationException("Stream is not yet created.");

            var (streamPath, streamArguments) = ParsePublishContext(command, clientContext);

            if (await AuthorizeAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments))
            {
                StartPublishing(clientContext, command, chunkStreamContext, streamPath, streamArguments);
            }

            return true;
        }

        private Task<bool> AuthorizeAsync(
            IRtmpClientContext clientContext,
            string streamPath,
            IDictionary<string, string> streamArguments,
            string publishingType)
        {
            return Task.FromResult(true);
        }

        private static (string StreamPath, IDictionary<string, string> StreamArguments)
            ParsePublishContext(RtmpPublishCommand command, IRtmpClientContext clientContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamPath(command.PublishingName);

            var streamPath = $"/{string.Join('/',
                new string[] { clientContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            return (streamPath, arguments);
        }

        private async Task<bool> AuthorizeAsync(
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            if (!await AuthorizeAsync(clientContext, streamPath, streamArguments, command.PublishingType))
            {
                _logger.AuthorizationFailed(clientContext.Client.ClientId, streamPath, command.PublishingType);
                SendAuthorizationFailedCommandMessage(clientContext, chunkStreamContext);
                return false;
            }

            return true;
        }

        private bool StartPublishing(
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            var startPublishingResult = _streamManager.StartPublishingStream(clientContext, streamPath, streamArguments, out _);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.PublishingStarted(clientContext.Client.ClientId, streamPath, command.PublishingType);
                    SendPublishingStartedMessage(clientContext, chunkStreamContext);
                    _eventDispatcher.RtmpStreamPublishedAsync(clientContext, streamPath, streamArguments.AsReadOnly());
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

        private void SendAuthorizationFailedCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                "Authorization failed.");
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
