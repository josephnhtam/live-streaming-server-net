using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
{
    public record RtmpCreateStreamCommand(double TransactionId, IDictionary<string, object> CommandObject);

    [RtmpCommand("createStream")]
    public class RtmpCreateStreamCommandHandler : RtmpCommandHandler<RtmpCreateStreamCommand>
    {
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;

        public RtmpCreateStreamCommandHandler(IRtmpCommandMessageSenderService commandMessageSender)
        {
            _commandMessageSender = commandMessageSender;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent @event,
            RtmpCreateStreamCommand command,
            CancellationToken cancellationToken)
        {
            RespondToClient(@event, command);
            return Task.FromResult(true);
        }

        public void RespondToClient(RtmpChunkEvent @event, RtmpCreateStreamCommand command)
        {
            var peerContext = @event.PeerContext;

            _commandMessageSender.SendCommandMessage(
                peerContext: @event.PeerContext,
                streamId: 0,
                commandName: "_result",
                transactionId: command.TransactionId,
                commandObject: null,
                parameters: [peerContext.NextPublishStreamId()]
            );
        }
    }
}
