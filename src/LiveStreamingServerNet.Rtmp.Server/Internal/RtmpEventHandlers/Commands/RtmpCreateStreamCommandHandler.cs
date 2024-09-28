using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpCreateStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("createStream")]
    internal class RtmpCreateStreamCommandHandler : RtmpCommandHandler<RtmpCreateStreamCommand, IRtmpClientSessionContext>
    {
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;

        public RtmpCreateStreamCommandHandler(IRtmpCommandMessageSenderService commandMessageSender)
        {
            _commandMessageSender = commandMessageSender;
        }

        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpCreateStreamCommand command,
            CancellationToken cancellationToken)
        {
            var streamContext = clientContext.CreateStreamContext();
            RespondToClient(clientContext, command, streamContext.StreamId);
            return ValueTask.FromResult(true);
        }

        public void RespondToClient(IRtmpClientSessionContext clientContext, RtmpCreateStreamCommand command, uint streamId)
        {
            _commandMessageSender.SendCommandMessage(
                clientContext: clientContext,
                messageStreamId: 0,
                chunkStreamId: 0,
                commandName: "_result",
                transactionId: command.TransactionId,
                commandObject: null,
                parameters: new List<object?> { streamId }
            );
        }
    }
}
