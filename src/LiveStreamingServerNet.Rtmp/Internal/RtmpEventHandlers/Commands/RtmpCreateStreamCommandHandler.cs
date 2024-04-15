using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpCreateStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("createStream")]
    internal class RtmpCreateStreamCommandHandler : RtmpCommandHandler<RtmpCreateStreamCommand>
    {
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;

        public RtmpCreateStreamCommandHandler(IRtmpCommandMessageSenderService commandMessageSender)
        {
            _commandMessageSender = commandMessageSender;
        }

        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpCreateStreamCommand command,
            CancellationToken cancellationToken)
        {
            if (clientContext.StreamId.HasValue)
                return ValueTask.FromResult(true);

            var streamId = clientContext.CreateNewStream();
            RespondToClient(clientContext, command, streamId);
            return ValueTask.FromResult(true);
        }

        public void RespondToClient(IRtmpClientContext clientContext, RtmpCreateStreamCommand command, uint streamId)
        {
            _commandMessageSender.SendCommandMessage(
                clientContext: clientContext,
                chunkStreamId: 0,
                commandName: "_result",
                transactionId: command.TransactionId,
                commandObject: null,
                parameters: new List<object?> { streamId }
            );
        }
    }
}
