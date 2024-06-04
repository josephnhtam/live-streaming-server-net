using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object>? Arguments);

    [RtmpCommand("connect")]
    internal class RtmpConnectCommandHandler : RtmpCommandHandler<RtmpConnectCommand>
    {
        private readonly IRtmpProtocolControlMessageSenderService _protocolControlMessageSender;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerConnectionEventDispatcher _eventDispatcher;
        private readonly RtmpServerConfiguration _config;
        private readonly ILogger _logger;

        public RtmpConnectCommandHandler(
            IRtmpProtocolControlMessageSenderService protocolControlMessageSender,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpServerConnectionEventDispatcher eventDispatcher,
            IOptions<RtmpServerConfiguration> config,
            ILogger<RtmpConnectCommandHandler> logger)
        {
            _protocolControlMessageSender = protocolControlMessageSender;
            _commandMessageSender = commandMessageSender;
            _eventDispatcher = eventDispatcher;
            _config = config.Value;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Connect(clientContext.Client.ClientId, command.CommandObject);

            if (!string.IsNullOrEmpty(clientContext.AppName))
            {
                _logger.ClientAlreadyConnected(clientContext.Client.ClientId);
                return false;
            }

            var appName = (string)command.CommandObject["app"];

            if (string.IsNullOrWhiteSpace(appName))
            {
                _logger.InvalidAppName(clientContext.Client.ClientId);
                return false;
            }

            clientContext.AppName = appName;

            _protocolControlMessageSender.SetChunkSize(clientContext, _config.OutChunkSize);
            _protocolControlMessageSender.WindowAcknowledgementSize(clientContext, _config.OutAcknowledgementWindowSize);
            _protocolControlMessageSender.SetClientBandwidth(clientContext, _config.ClientBandwidth, RtmpClientBandwidthLimitType.Dynamic);

            RespondToClient(clientContext, command);

            await _eventDispatcher.RtmpClientConnectedAsync(clientContext, command.CommandObject.AsReadOnly(), command.Arguments?.AsReadOnly());

            return true;
        }

        private void RespondToClient(IRtmpClientContext clientContext, RtmpConnectCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                clientContext: clientContext,
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
                        { RtmpArgumentNames.Level, RtmpArgumentValues.Status },
                        { RtmpArgumentNames.Code, RtmpStatusCodes.ConnectSuccess },
                        { RtmpArgumentNames.Description, "Connection succeeded." },
                        { RtmpArgumentNames.ObjectEncoding, 0 }
                    }
                }
            );
        }
    }
}
