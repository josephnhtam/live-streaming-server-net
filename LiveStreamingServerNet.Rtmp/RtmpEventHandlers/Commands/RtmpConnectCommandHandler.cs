using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Commands
{
    public record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Arguments);

    [RtmpCommand("connect")]
    public class RtmpConnectCommandHandler : RtmpCommandHandler<RtmpConnectCommand>
    {
        private readonly IRtmpProtocolControlMessageSenderService _protocolControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        public RtmpConnectCommandHandler(
            IRtmpProtocolControlMessageSenderService protocolControlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSender,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpConnectCommandHandler> logger)
        {
            _protocolControlMessageSender = protocolControlMessageSender;
            _commandMessageSender = commandMessageSender;
            _config = config.Value;
            _logger = logger;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Connect(peerContext.Peer.PeerId, command.CommandObject);

            peerContext.AppName = (string)command.CommandObject["app"];

            _protocolControlMessageSender.SetChunkSize(peerContext, _config.OutChunkSize);
            _protocolControlMessageSender.WindowAcknowledgementSize(peerContext, _config.OutAcknowledgementWindowSize);
            _protocolControlMessageSender.SetPeerBandwidth(peerContext, _config.PeerBandwidth, RtmpPeerBandwidthLimitType.Dynamic);

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
