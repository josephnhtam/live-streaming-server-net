using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
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
            RtmpChunkEvent @event,
            RtmpPublishCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("PeerId: {PeerId} | Publish: {PublishingName} | Type: {PublishingType}",
                @event.PeerContext.Peer.PeerId,
                !string.IsNullOrEmpty(command.PublishingName) ? command.PublishingName : "(Empty)",
                command.PublishingType);

            var peerContext = @event.PeerContext;

            UpdatePublishContext(command, peerContext);

            if (!await AuthorizeAsync(@event, command, peerContext))
                return false;

            if (!await StartPublishingAsync(@event, command, peerContext))
                return false;

            RespondToClient(peerContext);

            return true;
        }

        private Task<bool> AuthorizeAsync(IRtmpClientPeerContext peerContext, string publishingType)
        {
            return Task.FromResult(true);
        }

        private static void UpdatePublishContext(RtmpPublishCommand command, IRtmpClientPeerContext peerContext)
        {
            var (streamName, arguments) = ParseStreamName(command);

            peerContext.PublishStreamPath = $"/{string.Join('/',
                new string[] { peerContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            peerContext.PublishStreamArguments = arguments;
        }

        private async Task<bool> StartPublishingAsync(RtmpChunkEvent @event, RtmpPublishCommand command, IRtmpClientPeerContext peerContext)
        {
            var startPublishingResult = _serverContext.StartPublishingStream(peerContext.PublishStreamPath, peerContext);

            switch (startPublishingResult)
            {
                case StartPublishingStreamResult.Succeeded:
                    _logger.LogInformation("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Start publishing succeeded",
                        @event.PeerContext.Peer.PeerId, peerContext.PublishStreamPath, command.PublishingType);
                    return true;

                case StartPublishingStreamResult.AlreadyPublishing:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already publishing",
                        @event.PeerContext.Peer.PeerId, peerContext.PublishStreamPath, command.PublishingType);
                    await SendAlreadyPublishingCommandMessage(peerContext);
                    return false;

                case StartPublishingStreamResult.AlreadyExists:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Already exists",
                        @event.PeerContext.Peer.PeerId, peerContext.PublishStreamPath, command.PublishingType);
                    await SendAlreadyExistsCommandMessage(peerContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startPublishingResult), startPublishingResult, null);
            }
        }

        private async Task<bool> AuthorizeAsync(RtmpChunkEvent @event, RtmpPublishCommand command, IRtmpClientPeerContext peerContext)
        {
            if (!await AuthorizeAsync(peerContext, command.PublishingType))
            {
                _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Type: {PublishingType} | Authorization failed",
                    @event.PeerContext.Peer.PeerId, peerContext.PublishStreamPath, command.PublishingType);

                await SendAuthorizationFailedCommandMessage(peerContext);
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

        private async Task SendAlreadyExistsCommandMessage(IRtmpClientPeerContext peerContext)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                peerContext.PublishStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private async Task SendAlreadyPublishingCommandMessage(IRtmpClientPeerContext peerContext)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                peerContext.PublishStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadConnection,
                "Already publishing.");
        }

        private async Task SendAuthorizationFailedCommandMessage(IRtmpClientPeerContext peerContext)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                peerContext.PublishStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                "Authorization failed.");
        }

        private void RespondToClient(IRtmpClientPeerContext peerContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                peerContext,
                peerContext.PublishStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PublishStart,
                "Publishing started.");
        }
    }
}
