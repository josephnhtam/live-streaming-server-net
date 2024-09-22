using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpResultCommand(double TransactionId, IDictionary<string, object> CommandObject, object? Parameters);

    [RtmpCommand("_result")]
    internal class RtmpResultCommandHandler : RtmpCommandHandler<RtmpResultCommand, IRtmpSessionContext>
    {
        private readonly IRtmpCommandResultManagerService _resultManager;
        private readonly ILogger _logger;

        public RtmpResultCommandHandler(
            IRtmpCommandResultManagerService resultManager,
            ILogger<RtmpResultCommandHandler> logger)
        {
            _resultManager = resultManager;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            RtmpResultCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                if (command.TransactionId != 0)
                {
                    return await _resultManager.HandleCommandResultAsync(
                        context,
                        new RtmpCommandResult(
                            command.TransactionId,
                            command.CommandObject,
                            command.Parameters
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.CommandResultHandlingError(context.Session.Id, ex);
                return false;
            }

            return true;
        }
    }
}
