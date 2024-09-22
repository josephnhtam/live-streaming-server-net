using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal class RtmpCommandResultManagerService : IRtmpCommandResultManagerService
    {
        private readonly ConcurrentDictionary<double, Func<IRtmpSessionContext, RtmpCommandResult, Task<bool>>> _commandCallbacks = new();
        private uint _transactionId = 0;

        public double RegisterCommandCallback(Func<IRtmpSessionContext, RtmpCommandResult, Task<bool>> callback)
        {
            var transactionId = GetNextTransactionId();
            _commandCallbacks[transactionId] = callback;

            return transactionId;
        }

        public async ValueTask<bool> HandleCommandResultAsync(IRtmpSessionContext context, RtmpCommandResult result)
        {
            if (_commandCallbacks.TryRemove(result.TransactionId, out var callback))
                return await callback(context, result);

            return true;
        }

        private double GetNextTransactionId()
        {
            return Interlocked.Increment(ref _transactionId);
        }
    }
}
