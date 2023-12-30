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
            IRtmpClientPeerContext peerContext,
            RtmpCreateStreamCommand command,
            CancellationToken cancellationToken)
        {
            RespondToClient(peerContext, command);
            return Task.FromResult(true);
        }

        public void RespondToClient(IRtmpClientPeerContext peerContext, RtmpCreateStreamCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                peerContext: peerContext,
                streamId: 0,
                commandName: "_result",
                transactionId: command.TransactionId,
                commandObject: null,
                parameters: [peerContext.NextPublishStreamId()]
            );
        }
    }
}
