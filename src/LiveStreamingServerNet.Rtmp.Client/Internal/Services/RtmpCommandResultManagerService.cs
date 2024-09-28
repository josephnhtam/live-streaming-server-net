using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal class RtmpCommandResultManagerService : IRtmpCommandResultManagerService, IClientEventHandler, IDisposable
    {
        private readonly ConcurrentDictionary<double, (CommandCallbackDelegate Callback, Action? CancellationCallback)> _commandCallbacks = new();

        private uint _transactionId = 0;
        private bool _stopped;

        private object _syncLock = new();

        public double RegisterCommandCallback(CommandCallbackDelegate callback, Action? cancellationCallback)
        {
            var transactionId = GetNextTransactionId();
            var registered = false;

            lock (_syncLock)
            {
                if (!_stopped)
                {
                    _commandCallbacks[transactionId] = (callback, cancellationCallback);
                    registered = true;
                }
            }

            if (!registered)
            {
                cancellationCallback?.Invoke();
            }

            return transactionId;
        }

        public async ValueTask<bool> HandleCommandResultAsync(IRtmpSessionContext context, RtmpCommandResult result)
        {
            if (_commandCallbacks.TryRemove(result.TransactionId, out var callback))
                return await callback.Callback(context, result);

            return true;
        }

        private double GetNextTransactionId()
        {
            return Interlocked.Increment(ref _transactionId);
        }

        private void Cleanup()
        {
            lock (_syncLock)
            {
                _stopped = true;
            }

            foreach (var callback in _commandCallbacks.Values)
            {
                callback.CancellationCallback?.Invoke();
            }

            _commandCallbacks.Clear();
        }

        public void Dispose()
        {
            Cleanup();
        }

        public Task OnClientStoppedAsync(IEventContext context)
        {
            Cleanup();
            return Task.CompletedTask;
        }

        public Task OnClientConnectedAsync(IEventContext context, ISessionHandle session)
            => Task.CompletedTask;
    }
}
