﻿using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Attributes;
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

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpCreateStreamCommand command,
            CancellationToken cancellationToken)
        {
            RespondToClient(clientContext, command);
            return Task.FromResult(true);
        }

        public void RespondToClient(IRtmpClientContext clientContext, RtmpCreateStreamCommand command)
        {
            _commandMessageSender.SendCommandMessage(
                clientContext: clientContext,
                chunkStreamId: 0,
                commandName: "_result",
                transactionId: command.TransactionId,
                commandObject: null,
                parameters: [clientContext.CreateNewStream()]
            );
        }
    }
}
