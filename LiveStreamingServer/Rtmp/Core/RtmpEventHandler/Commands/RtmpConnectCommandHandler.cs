using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
using System.Text.Json;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
{
    public record RtmpConnectCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Arguments);

    [RtmpCommand("connect")]
    public class RtmpConnectCommandHandler : RtmpCommandHandler<RtmpConnectCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            RtmpChunkEvent message,
            RtmpConnectCommand command,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(command.TransactionId);
            Console.WriteLine(JsonSerializer.Serialize(command.CommandObject));
            return Task.FromResult(true);
        }
    }
}
