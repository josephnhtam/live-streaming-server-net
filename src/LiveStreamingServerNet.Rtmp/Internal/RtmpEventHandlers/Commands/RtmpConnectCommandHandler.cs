using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Arguments);

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

        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Connect(clientContext.Client.ClientId, command.CommandObject);

            clientContext.AppName = (string)command.CommandObject["app"];

            _protocolControlMessageSender.SetChunkSize(clientContext, _config.OutChunkSize);
            _protocolControlMessageSender.WindowAcknowledgementSize(clientContext, _config.OutAcknowledgementWindowSize);
            _protocolControlMessageSender.SetClientBandwidth(clientContext, _config.ClientBandwidth, RtmpClientBandwidthLimitType.Dynamic);

            RespondToClient(clientContext, command);

            _eventDispatcher.RtmpClientConnectedAsync(clientContext, command.CommandObject.AsReadOnly(), command.Arguments?.AsReadOnly());

            return ValueTask.FromResult(true);
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
