using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Utilities;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    public record RtmpPublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName, string PublishingType);

    [RtmpCommand("publish")]
    public class RtmpPublishCommandHandler : RtmpCommandHandler<RtmpPublishCommand>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly ILogger _logger;

        public RtmpPublishCommandHandler(IRtmpStreamManagerService streamManager, IRtmpCommandMessageSenderService commandMessageSender, ILogger<RtmpPublishCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _logger = logger;
        }

        public override async Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpPublishCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("PeerId: {PeerId} | Publish: {PublishingName} | Type: {PublishingType}",
                peerContext.Peer.PeerId,
                !string.IsNullOrEmpty(command.PublishingName) ? command.PublishingName : "(Empty)",
                command.PublishingType);

            if (peerContext.StreamId == null)
                throw new InvalidOperationException("Stream is not yet created.");

            var (streamPath, streamArguments) = ParsePublishContext(command, peerContext);

            if (await AuthorizeAsync(peerContext, command, chunkStreamContext, streamPath, streamArguments))
            {
                StartPublishing(peerContext, command, chunkStreamContext, streamPath, streamArguments);
            }

            return true;
        }

        private Task<bool> AuthorizeAsync(
            IRtmpClientPeerContext peerContext,
            string streamPath,
            IDictionary<string, string> streamArguments,
            string publishingType)
        {
            return Task.FromResult(true);
        }

        private static (string StreamPath, IDictionary<string, string> StreamArguments)
            ParsePublishContext(RtmpPublishCommand command, IRtmpClientPeerContext peerContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamPath(command.PublishingName);

            var streamPath = $"/{string.Join('/',
                new string[] { peerContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            return (streamPath, arguments);
        }

        private async Task<bool> AuthorizeAsync(
            IRtmpClientPeerContext peerContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            if (!await AuthorizeAsync(peerContext, streamPath, streamArguments, command.PublishingType))
            {
                _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Authorization failed",
                    peerContext.Peer.PeerId, streamPath, command.PublishingType);

                SendAuthorizationFailedCommandMessage(peerContext, chunkStreamContext);
                return false;
            }

            return true;
        }

        private bool StartPublishing(
            IRtmpClientPeerContext peerContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            var startPublishingResult = _streamManager.StartPublishingStream(peerContext, streamPath, streamArguments, out _);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.LogInformation("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Start publishing successfully",
                        peerContext.Peer.PeerId, streamPath, command.PublishingType);
                    SendPublishingStartedMessage(peerContext, chunkStreamContext);
                    return true;

                case PublishingStreamResult.AlreadySubscribing:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already subscribing",
                        peerContext.Peer.PeerId, streamPath, command.PublishingType);
                    SendBadConnectionCommandMessage(peerContext, chunkStreamContext, "Already subscribing.");
                    return false;

                case PublishingStreamResult.AlreadyPublishing:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already publishing",
                        peerContext.Peer.PeerId, streamPath, command.PublishingType);
                    SendBadConnectionCommandMessage(peerContext, chunkStreamContext, "Already publishing.");
                    return false;

                case PublishingStreamResult.AlreadyExists:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already exists",
                        peerContext.Peer.PeerId, streamPath, command.PublishingType);
                    SendAlreadyExistsCommandMessage(peerContext, chunkStreamContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startPublishingResult), startPublishingResult, null);
            }
        }

        private void SendAlreadyExistsCommandMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private void SendBadConnectionCommandMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadConnection,
                reason);
        }

        private void SendAuthorizationFailedCommandMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                "Authorization failed.");
        }

        private void SendPublishingStartedMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PublishStart,
                "Publishing started.");
        }
    }
}
