using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpOnStatusCommand(double TransactionId, IDictionary<string, object> CommandObject, object? Parameters);

    [RtmpCommand("onStatus")]
    internal class RtmpOnStatusCommandHandler : RtmpCommandHandler<RtmpOnStatusCommand, IRtmpSessionContext>
    {
        private readonly IRtmpCommandResultManagerService _resultManager;
        private readonly ILogger _logger;

        public RtmpOnStatusCommandHandler(
            IRtmpCommandResultManagerService resultManager,
            ILogger<RtmpResultCommandHandler> logger)
        {
            _resultManager = resultManager;
            _logger = logger;
        }

        public override ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            RtmpOnStatusCommand command,
            CancellationToken cancellationToken)
        {
            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var streamContext = context.GetStreamContext(streamId);

            if (command.Parameters is not IDictionary<string, object> parameters)
            {
                return ValueTask.FromResult(true);
            }

            var level = parameters.GetValueOrDefault<string>(RtmpArguments.Level) ?? string.Empty;
            var code = parameters.GetValueOrDefault<string>(RtmpArguments.Code) ?? string.Empty;
            var description = parameters.GetValueOrDefault<string>(RtmpArguments.Description) ?? string.Empty;

            if (streamContext != null)
            {
                streamContext.ReceiveStatus(new(level, code, description));
            }

            return ValueTask.FromResult(true);
        }
    }
}
