using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
{
    public record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Arguments);

    [RtmpCommand("connect")]
    public class RtmpConnectCommandHandler : RtmpCommandHandler<RtmpConnectCommand>
    {
        private readonly IRtmpControlMessageSenderService _controlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly ILogger<RtmpConnectCommandHandler> _logger;

        public RtmpConnectCommandHandler(
            IRtmpControlMessageSenderService controlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSender,
            ILogger<RtmpConnectCommandHandler> logger)
        {
            _controlMessageSender = controlMessageSender;
            _commandMessageSender = commandMessageSender;
            _logger = logger;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("PeerId: {PeerId} | Connect: {CommandObject}", @event.PeerContext.Peer.PeerId, JsonSerializer.Serialize(command.CommandObject));

            var peerContext = @event.PeerContext;

            _controlMessageSender.SetChunkSize(peerContext, RtmpConstants.DefaultChunkSize);
            _controlMessageSender.WindowAcknowledgementSize(peerContext, RtmpConstants.DefaultInAcknowledgementWindowSize);
            _controlMessageSender.SetPeerBandwidth(peerContext, RtmpConstants.DefaultPeerBandwidth, RtmpPeerBandwidthLimitType.Dynamic);

            RespondToClient(@event, command);

            return Task.FromResult(true);
        }

        public void RespondToClient(RtmpChunkEvent @event, RtmpConnectCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                peerContext: @event.PeerContext,
                streamId: 3,
                commandName: "_result",
                transactionId: command.TransactionId,
                parameters: [
                    new Dictionary<string, object>
                    {
                        { "fmsVer", "LS/1,0,0,000" },
                        { "capabilities", 31 },
                        { "mode", 1 }
                    },
                    new Dictionary<string, object>
                    {
                        { "level", "status" },
                        { "code", "NetConnection.Connect.Success" },
                        { "description", "Connection succeeded." },
                        { "objectEncoding", 0 }
                    }
                ]
            );
        }
    }
}
