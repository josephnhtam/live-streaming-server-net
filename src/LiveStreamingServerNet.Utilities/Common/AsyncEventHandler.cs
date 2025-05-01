using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Utilities.Common
{
    public class AsyncEventHandler<TEventArgs> : IAsyncEventHandler<TEventArgs>
    {
        private readonly List<AsyncEventHandlerDelegate<TEventArgs>> _handlers = new();
        private readonly List<AsyncEventHandlerDelegate<TEventArgs>> _currentHandlers = new();
        private readonly object _lock = new();

        public void Register(AsyncEventHandlerDelegate<TEventArgs> handler)
        {
            lock (_lock)
            {
                _handlers.Add(handler);
            }
        }

        public void Unregister(AsyncEventHandlerDelegate<TEventArgs> handler)
        {
            lock (_lock)
            {
                _handlers.Remove(handler);
            }
        }

        public async ValueTask InvokeAsync(object? sender, TEventArgs e)
        {
            lock (_lock)
            {
                _currentHandlers.Clear();
                _currentHandlers.AddRange(_handlers);
            }

            await Task.WhenAll(_currentHandlers.Select(async h =>
            {
                try
                {
                    await h.Invoke(sender, e);
                }
                catch { }
            }));
        }
    }
}
