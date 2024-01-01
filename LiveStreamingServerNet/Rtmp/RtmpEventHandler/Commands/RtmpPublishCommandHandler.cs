using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Utilities;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Commands
{
    public record RtmpPublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName, string PublishingType);

    [RtmpCommand("publish")]
    public class RtmpPublishCommandHandler : RtmpCommandHandler<RtmpPublishCommand>
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly ILogger _logger;

        public RtmpPublishCommandHandler(IRtmpServerContext serverContext, IRtmpCommandMessageSenderService commandMessageSender, ILogger<RtmpPublishCommandHandler> logger)
        {
            _serverContext = serverContext;
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

            if (peerContext.PublishStreamContext == null)
                throw new InvalidOperationException("Stream is not created yet.");

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
            var startPublishingResult = _serverContext.StartPublishingStream(peerContext, streamPath, streamArguments);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.LogInformation("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Start publishing successfully",
                        peerContext.Peer.PeerId, streamPath, command.PublishingType);
                    SendPublishingStartedMessage(peerContext, chunkStreamContext);
                    return true;

                case PublishingStreamResult.AlreadyPublishing:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already publishing",
                        peerContext.Peer.PeerId, streamPath, command.PublishingType);
                    SendAlreadyPublishingCommandMessage(peerContext, chunkStreamContext);
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

        private void SendAlreadyPublishingCommandMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadConnection,
                "Already publishing.");
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
