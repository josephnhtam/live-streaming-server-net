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
            RtmpChunkEvent message,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("PeerId: {PeerId} | Connect: {CommandObject}", message.PeerContext.Peer.PeerId, JsonSerializer.Serialize(command.CommandObject));

            var peerContext = message.PeerContext;

            _controlMessageSender.SetChunkSize(peerContext, RtmpConstants.DefaultChunkSize);
            RespondToClient(message, command);

            return Task.FromResult(true);
        }

        public void RespondToClient(RtmpChunkEvent message, RtmpConnectCommand command)
        {
            var peerContext = message.PeerContext;

            _commandMessageSender.SendCommandMessage(peerContext, 3, "_result", command.TransactionId,
                [
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
