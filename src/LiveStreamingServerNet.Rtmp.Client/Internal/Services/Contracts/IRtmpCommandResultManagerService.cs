using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommandResultManagerService
    {
        double RegisterCommandCallback(CommandCallbackDelegate callback, Action? cancellationCallback = null);
        ValueTask<bool> HandleCommandResultAsync(IRtmpSessionContext context, RtmpCommandResult result);
    }

    internal delegate Task<bool> CommandCallbackDelegate(IRtmpSessionContext Context, RtmpCommandResult Result);
    internal record struct RtmpCommandResult(double TransactionId, IDictionary<string, object> CommandObject, IList<object>? Parameters);
}
