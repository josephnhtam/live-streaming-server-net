namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    public interface IAsyncEventHandler<TEventArgs>
    {
        void Register(AsyncEventHandlerDelegate<TEventArgs> handler);
        void Unregister(AsyncEventHandlerDelegate<TEventArgs> handler);
    }

    public delegate Task AsyncEventHandlerDelegate<TEventArgs>(object? sender, TEventArgs e);
}
