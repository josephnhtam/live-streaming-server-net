using LiveStreamingClientNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal partial class RtmpCommanderService : IRtmpCommanderService
    {
        private readonly IRtmpCommandResultManagerService _commandResultManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpClientConnectionEventDispatcher _connectionEventDispatcher;
        private readonly IRtmpClientStreamEventDispatcher _streamEventDispatcher;

        public RtmpCommanderService(
            IRtmpCommandResultManagerService commandResultManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpClientConnectionEventDispatcher connectionEventDispatcher,
            IRtmpClientStreamEventDispatcher streamEventDispatcher)
        {
            _commandResultManager = commandResultManager;
            _commandMessageSender = commandMessageSender;
            _connectionEventDispatcher = connectionEventDispatcher;
            _streamEventDispatcher = streamEventDispatcher;
        }

        public void Command(RtmpCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                command.chunkStreamId,
                command.commandName,
                transactionId: 0,
                command.commandObject,
                command.parameters,
                command.amfEncodingType
            );
        }

        public void Command(RtmpCommand command, Func<IRtmpSessionContext, RtmpCommandResult, Task<bool>> callback)
        {
            var transactionId = _commandResultManager.RegisterCommandCallback(callback);

            _commandMessageSender.SendCommandMessage(
               command.chunkStreamId,
               command.commandName,
               transactionId,
               command.commandObject,
               command.parameters,
               command.amfEncodingType
           );
        }

        public void Connect(string appName, IDictionary<string, object>? information = null)
        {
            var commandObject = information == null ?
                new Dictionary<string, object>() :
                new Dictionary<string, object>(information);

            commandObject["app"] = appName;

            var command = new RtmpCommand(
               chunkStreamId: 3,
               commandName: "connect",
               commandObject: commandObject
            );

            Command(command, async (context, result) =>
            {
                if (!string.IsNullOrEmpty(context.AppName))
                {
                    return false;
                }

                context.AppName = appName;

                if (result.CommandObject.TryGetValue(RtmpArgumentNames.Code, out var code) && code is string codeString)
                {
                    if (codeString == RtmpStatusCodes.ConnectSuccess)
                    {
                        await _connectionEventDispatcher.RtmpConnectedAsync(context, result.CommandObject, result.Parameters);
                        return true;
                    }

                    await _connectionEventDispatcher.RtmpConnectionRejectedAsync(context, result.CommandObject, result.Parameters);
                    return false;
                }

                await _connectionEventDispatcher.RtmpConnectedAsync(context, result.CommandObject, result.Parameters);
                return true;
            });
        }

        public void CreateStream()
        {
            var command = new RtmpCommand(
                chunkStreamId: 3,
                commandName: "createStream",
                commandObject: new Dictionary<string, object>()
            );

            Command(command, async (context, result) =>
            {
                if (context.StreamId.HasValue)
                {
                    return false;
                }

                if (result.Parameters is not double streamId)
                {
                    return false;
                }

                context.StreamId = (uint)streamId;

                await _streamEventDispatcher.RtmpStreamCreated(context, context.StreamId.Value);
                return true;
            });
        }
    }
}
