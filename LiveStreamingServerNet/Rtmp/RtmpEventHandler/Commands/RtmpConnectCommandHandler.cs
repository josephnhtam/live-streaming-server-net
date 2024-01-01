using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.Commands
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
            IRtmpClientPeerContext peerContext,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("PeerId: {PeerId} | Connect: {CommandObject}", peerContext.Peer.PeerId, JsonSerializer.Serialize(command.CommandObject));

            peerContext.AppName = (string)command.CommandObject["app"];

            _controlMessageSender.SetChunkSize(peerContext, RtmpConstants.DefaultChunkSize);
            _controlMessageSender.WindowAcknowledgementSize(peerContext, RtmpConstants.DefaultInAcknowledgementWindowSize);
            _controlMessageSender.SetPeerBandwidth(peerContext, RtmpConstants.DefaultPeerBandwidth, RtmpPeerBandwidthLimitType.Dynamic);

            RespondToClient(peerContext, command);

            return Task.FromResult(true);
        }

        private void RespondToClient(IRtmpClientPeerContext peerContext, RtmpConnectCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                peerContext: peerContext,
                chunkStreamId: 3,
                commandName: "_result",
                transactionId: command.TransactionId,
                commandObject:
                    new Dictionary<string, object>
                    {
                        { "fmsVer", "LS/1,0,0,000" },
                        { "capabilities", 31 },
                        { "mode", 1 }
                    },
                parameters: [
                    new Dictionary<string, object>
                    {
                        { RtmpArgumentNames.Level, RtmpArgumentValues.Status },
                        { RtmpArgumentNames.Code, RtmpStatusCodes.ConnectSuccess },
                        { RtmpArgumentNames.Description, "Connection succeeded." },
                        { RtmpArgumentNames.ObjectEncoding, 0 }
                    }
                ]
            );
        }
    }
}
