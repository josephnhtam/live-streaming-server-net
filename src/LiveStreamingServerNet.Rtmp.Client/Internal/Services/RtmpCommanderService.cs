using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal partial class RtmpCommanderService : IRtmpCommanderService
    {
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpCommandResultManagerService _commandResultManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;

        public RtmpCommanderService(
            IRtmpClientContext clientContext,
            IRtmpCommandResultManagerService commandResultManager,
            IRtmpCommandMessageSenderService commandMessageSender)
        {
            _clientContext = clientContext;
            _commandResultManager = commandResultManager;
            _commandMessageSender = commandMessageSender;
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

                if (result.CommandObject.TryGetValue(RtmpArguments.Code, out var code) && code is string codeString)
                {
                    if (codeString == RtmpConnectionStatusCodes.ConnectSuccess)
                    {
                        await (callback?.Invoke(true, result.CommandObject, result.Parameters) ?? ValueTask.CompletedTask);
                        return true;
                    }

                    await (callback?.Invoke(false, result.CommandObject, result.Parameters) ?? ValueTask.CompletedTask);
                    return false;
                }

                await (callback?.Invoke(true, result.CommandObject, result.Parameters) ?? ValueTask.CompletedTask);
                return true;
            }, cancellationCallback);
        }

        public void CreateStream(CreateStreamCallbackDelegate? callback, Action? cancellationCallback)
        {
            var command = new RtmpCommand(
                messageStreamId: 0,
                chunkStreamId: 3,
                commandName: "createStream",
                commandObject: null
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

                    await (callback?.Invoke(true, streamContext) ?? ValueTask.CompletedTask);
                    return true;
                }
                catch
                {
                    await (callback?.Invoke(false, null) ?? ValueTask.CompletedTask);
                    return false;
                }
            }, cancellationCallback);
        }

        public void CloseStream(uint streamId)
        {
            var sessionContext = GetSessionContext();
            var streamContext = sessionContext.GetStreamContext(streamId);

            if (streamContext == null)
            {
                throw new InvalidOperationException("Stream does not exist.");
            }

            streamContext.RemovePublishContext();
            streamContext.RemoveSubscribeContext();

            var command = new RtmpCommand(
                messageStreamId: streamId,
                chunkStreamId: 3,
                commandName: "closeStream",
                commandObject: null
            );

            Command(command);
        }

        public void DeleteStream(uint streamId)
        {
            var sessionContext = GetSessionContext();
            var streamContext = sessionContext.GetStreamContext(streamId);

            if (streamContext == null)
            {
                throw new InvalidOperationException("Stream does not exist.");
            }

            streamContext.RemovePublishContext();
            streamContext.RemoveSubscribeContext();
            sessionContext.RemoveStreamContext(streamId);

            var command = new RtmpCommand(
                messageStreamId: streamId,
                chunkStreamId: 3,
                commandName: "deleteStream",
                commandObject: null,
                parameters: new List<object?> { (double)streamId }
            );

            Command(command);
        }

        public void Play(uint streamId, string streamName, double start, double duration, bool reset)
        {
            var sessionContext = GetSessionContext();
            var streamContext = sessionContext.GetStreamContext(streamId);

            if (streamContext == null)
            {
                throw new InvalidOperationException("Stream does not exist.");
            }

            streamContext.CreateSubscribeContext();

            var command = new RtmpCommand(
                messageStreamId: streamId,
                chunkStreamId: 3,
                commandName: "play",
                commandObject: null,
                parameters: new List<object?> { streamName, start, duration, reset }
            );

            Command(command);
        }

        private IRtmpSessionContext GetSessionContext()
            => _clientContext.SessionContext ?? throw new InvalidOperationException("Session is not available.");
    }
}
