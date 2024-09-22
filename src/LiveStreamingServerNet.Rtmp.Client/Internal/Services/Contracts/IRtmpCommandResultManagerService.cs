using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommandResultManagerService
    {
        double RegisterCommandCallback(Func<IRtmpSessionContext, RtmpCommandResult, Task<bool>> callback);
        ValueTask<bool> HandleCommandResultAsync(IRtmpSessionContext context, RtmpCommandResult result);
    }

    internal record struct RtmpCommandResult(double TransactionId, IDictionary<string, object> CommandObject, object? Parameters);
}
