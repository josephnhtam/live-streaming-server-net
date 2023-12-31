using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using LiveStreamingServer.Rtmp.Core.Services.Extensions;
using Microsoft.Extensions.Logging;
using System.Web;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
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

            var publishStreamContext = peerContext.PublishStreamContext ??
                throw new InvalidOperationException("Stream is not created yet.");

            UpdatePublishContext(command, peerContext, publishStreamContext);

            if (!await AuthorizeAsync(peerContext, command, publishStreamContext))
                return false;

            if (!await StartPublishingAsync(peerContext, command, publishStreamContext))
                return false;

            RespondToClient(peerContext, publishStreamContext);

            return true;
        }

        private Task<bool> AuthorizeAsync(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext, string publishingType)
        {
            return Task.FromResult(true);
        }

        private static void UpdatePublishContext(RtmpPublishCommand command, IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext)
        {
            var (streamName, arguments) = ParseStreamName(command);

            publishStreamContext.StreamPath = $"/{string.Join('/',
                new string[] { peerContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            publishStreamContext.StreamArguments = arguments;
        }

        private async Task<bool> StartPublishingAsync(IRtmpClientPeerContext peerContext, RtmpPublishCommand command, IRtmpPublishStreamContext publishStreamContext)
        {
            var startPublishingResult = _serverContext.StartPublishingStream(publishStreamContext.StreamPath, peerContext);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.LogInformation("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Start publishing successfully",
                        peerContext.Peer.PeerId, publishStreamContext.StreamPath, command.PublishingType);
                    return true;

                case PublishingStreamResult.AlreadyPublishing:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already publishing",
                        peerContext.Peer.PeerId, publishStreamContext.StreamPath, command.PublishingType);
                    await SendAlreadyPublishingCommandMessage(peerContext, publishStreamContext);
                    return false;

                case PublishingStreamResult.AlreadyExists:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already exists",
                        peerContext.Peer.PeerId, publishStreamContext.StreamPath, command.PublishingType);
                    await SendAlreadyExistsCommandMessage(peerContext, publishStreamContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startPublishingResult), startPublishingResult, null);
            }
        }

        private async Task<bool> AuthorizeAsync(IRtmpClientPeerContext peerContext, RtmpPublishCommand command, IRtmpPublishStreamContext publishStreamContext)
        {
            if (!await AuthorizeAsync(peerContext, publishStreamContext, command.PublishingType))
            {
                _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Authorization failed",
                    peerContext.Peer.PeerId, publishStreamContext.StreamPath, command.PublishingType);

                await SendAuthorizationFailedCommandMessage(peerContext, publishStreamContext);
                return false;
            }

            return true;
        }

        private static (string, IDictionary<string, string>) ParseStreamName(RtmpPublishCommand command)
        {
            var publishingNameSplit = command.PublishingName.Split('?');

            var streamName = publishingNameSplit[0];

            var queryString = publishingNameSplit.Length > 1 ? publishingNameSplit[1] : string.Empty;
            var queryStringCollection = HttpUtility.ParseQueryString(queryString);
            var queryStringMap = queryStringCollection
                .AllKeys
                .Where(key => !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(queryStringCollection[key]))
                .ToDictionary(key => key!, key => queryStringCollection[key]!);

            return (streamName, queryStringMap);
        }

        private async Task SendAlreadyExistsCommandMessage(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                publishStreamContext.StreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private async Task SendAlreadyPublishingCommandMessage(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                publishStreamContext.StreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadConnection,
                "Already publishing.");
        }

        private async Task SendAuthorizationFailedCommandMessage(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                publishStreamContext.StreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                "Authorization failed.");
        }

        private void RespondToClient(IRtmpClientPeerContext peerContext, IRtmpPublishStreamContext publishStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                publishStreamContext.StreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PublishStart,
                "Publishing started.");
        }
    }
}
