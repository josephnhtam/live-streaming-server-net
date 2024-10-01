using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommandResultManagerService
    {
        double RegisterCommandCallback(CommandCallbackDelegate callback, Action? cancellationCallback = null);
        ValueTask<bool> HandleCommandResultAsync(IRtmpSessionContext context, RtmpCommandResponse response);
    }

    internal record struct RtmpCommandResponse(double TransactionId, IDictionary<string, object> CommandObject, IList<object>? Parameters);
    internal delegate Task<bool> CommandCallbackDelegate(IRtmpSessionContext Context, RtmpCommandResponse Response);
}
