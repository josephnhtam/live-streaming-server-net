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
                command.messageStreamId,
                command.chunkStreamId,
                command.commandName,
                transactionId: 0,
                command.commandObject,
                command.parameters,
                command.amfEncodingType
            );
        }

        public void Command(RtmpCommand command, CommandCallbackDelegate callback, Action? cancellationCallback = null)
        {
            var transactionId = _commandResultManager.RegisterCommandCallback(callback, cancellationCallback);

            _commandMessageSender.SendCommandMessage(
               command.messageStreamId,
               command.chunkStreamId,
               command.commandName,
               transactionId,
               command.commandObject,
               command.parameters,
               command.amfEncodingType
           );
        }

        public void Connect(string appName, IDictionary<string, object>? information, ConnectCallbackDelegate? callback, Action? cancellationCallback)
        {
            var commandObject = information == null ?
                new Dictionary<string, object>() :
                new Dictionary<string, object>(information);

            commandObject["app"] = appName;

            var command = new RtmpCommand(
                messageStreamId: 0,
                chunkStreamId: 3,
                commandName: "connect",
                commandObject: commandObject
            );

            Command(command, async (context, result) =>
            {
                context.AppName = appName;

                if (result.CommandObject.TryGetValue(RtmpArgumentNames.Code, out var code) && code is string codeString)
                {
                    if (codeString == RtmpStatusCodes.ConnectSuccess)
                    {
                        await HandleConnectResultAsync(true, context, result, callback);
                        return true;
                    }

                    await HandleConnectResultAsync(false, context, result, callback);
                    return false;
                }

                await HandleConnectResultAsync(true, context, result, callback);
                return true;
            }, cancellationCallback);

            async Task HandleConnectResultAsync(bool success, IRtmpSessionContext context, RtmpCommandResult result, ConnectCallbackDelegate? callback)
            {
                if (success)
                    await _connectionEventDispatcher.RtmpConnectedAsync(context, result.CommandObject, result.Parameters);
                else
                    await _connectionEventDispatcher.RtmpConnectionRejectedAsync(context, result.CommandObject, result.Parameters);

                if (callback != null)
                    await callback.Invoke(success, result.CommandObject, result.Parameters);
            }
        }

        public void CreateStream(CreateStreamCallbackDelegate? callback, Action? cancellationCallback)
        {
            var command = new RtmpCommand(
                messageStreamId: 0,
                chunkStreamId: 3,
                commandName: "createStream",
                commandObject: new Dictionary<string, object>()
            );

            Command(command, async (context, result) =>
            {
                if (result.Parameters is not double streamIdNumber)
                {
                    callback?.Invoke(false, null);
                    return false;
                }

                try
                {
                    var streamId = (uint)streamIdNumber;
                    var streamContext = context.CreateStreamContext(streamId);

                    await _streamEventDispatcher.RtmpStreamCreated(context, streamId);
                    callback?.Invoke(true, streamContext);
                    return true;
                }
                catch
                {
                    callback?.Invoke(false, null);
                    return false;
                }
            }, cancellationCallback);
        }

        public void Play(uint streamId, string streamName, double start, double duration, bool reset)
        {
            var command = new RtmpCommand(
                messageStreamId: streamId,
                chunkStreamId: 3,
                commandName: "play",
                commandObject: new Dictionary<string, object>(),
                parameters: new List<object?> { streamName, start, duration, reset }
            );

            Command(command);
        }
    }
}
