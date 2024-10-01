using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object>? Arguments);

    [RtmpCommand("connect")]
    internal class RtmpConnectCommandHandler : RtmpCommandHandler<RtmpConnectCommand, IRtmpClientSessionContext>
    {
        private readonly IRtmpProtocolControlService _protocolControl;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        public RtmpConnectCommandHandler(
            IRtmpProtocolControlService protocolControl,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpConnectCommandHandler> logger)
        {
            _protocolControl = protocolControl;
            _commandMessageSender = commandMessageSender;
            _eventDispatcher = eventDispatcher;
            _config = config.Value;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Connect(clientContext.Client.Id, command.CommandObject);

            if (!string.IsNullOrEmpty(clientContext.AppName))
            {
                _logger.ClientAlreadyConnected(clientContext.Client.Id);
                return false;
            }

            var appName = (string)command.CommandObject["app"];

            if (string.IsNullOrWhiteSpace(appName))
            {
                _logger.InvalidAppName(clientContext.Client.Id);
                return false;
            }

            clientContext.AppName = appName;

            _protocolControl.SetChunkSize(clientContext, _config.OutChunkSize);
            _protocolControl.WindowAcknowledgementSize(clientContext, _config.WindowAcknowledgementSize);
            _protocolControl.SetPeerBandwidth(clientContext, _config.PeerBandwidth, RtmpBandwidthLimitType.Dynamic);

            RespondToClient(clientContext, command);

            await _eventDispatcher.RtmpClientConnectedAsync(clientContext, command.CommandObject.AsReadOnly(), command.Arguments?.AsReadOnly());

            return true;
        }

        private void RespondToClient(IRtmpClientSessionContext clientContext, RtmpConnectCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                clientContext: clientContext,
                messageStreamId: 0,
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
                parameters: new List<object?>
                {
                    new Dictionary<string, object>
                    {
                        { RtmpArguments.Level, RtmpStatusLevels.Status },
                        { RtmpArguments.Code, RtmpConnectionStatusCodes.ConnectSuccess },
                        { RtmpArguments.Description, "Connection succeeded." },
                        { RtmpArguments.ObjectEncoding, 0 }
                    }
                }
            );
        }
    }
}
