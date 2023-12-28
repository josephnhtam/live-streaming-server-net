using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using System.Text.Json;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
{
    public record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Arguments);

    [RtmpCommand("connect")]
    public class RtmpConnectCommandHandler : RtmpCommandHandler<RtmpConnectCommand>
    {
        private readonly IRtmpControlMessageSenderService _controlMessageSenderService;

        public RtmpConnectCommandHandler(IRtmpControlMessageSenderService controlMessageSenderService)
        {
            _controlMessageSenderService = controlMessageSenderService;
        }

        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent message,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(command.TransactionId);
            Console.WriteLine(JsonSerializer.Serialize(command.CommandObject));

            var peerContext = message.PeerContext;

            _controlMessageSenderService.SetChunkSize(peerContext, RtmpConstants.DefaultChunkSize);

            return Task.FromResult(true);
        }
    }
}
