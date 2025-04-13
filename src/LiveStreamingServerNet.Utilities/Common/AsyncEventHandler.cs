using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Utilities.Common
{
    public class AsyncEventHandler<TEventArgs> : IAsyncEventHandler<TEventArgs>
    {
        private readonly List<AsyncEventHandlerDelegate<TEventArgs>> _handlers = new();
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
            AsyncEventHandlerDelegate<TEventArgs>[] handlers;

            lock (_lock)
            {
                handlers = _handlers.ToArray();
            }

            await Task.WhenAll(handlers.Select(async h =>
            {
                try
                {
                    await h.Invoke(sender, e);
                }
                catch { }
            }));
        }
    }

    public delegate Task AsyncEventHandlerDelegate<TEventArgs>(object? sender, TEventArgs e);
}
