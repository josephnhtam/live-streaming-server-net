namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    /// <summary>
    /// Defines an asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">
    /// The type of event arguments to be provided when the event is raised.
    /// </typeparam>
    public interface IAsyncEventHandler<TEventArgs>
    {
        /// <summary>
        /// Registers an asynchronous event handler delegate.
        /// </summary>
        /// <param name="handler">
        /// The asynchronous event handler delegate to be invoked when the event is raised.
        /// </param>
        void Register(AsyncEventHandlerDelegate<TEventArgs> handler);

        /// <summary>
        /// Unregisters a previously registered asynchronous event handler delegate.
        /// </summary>
        /// <param name="handler">
        /// The asynchronous event handler delegate to remove from the event handler.
        /// </param>
        void Unregister(AsyncEventHandlerDelegate<TEventArgs> handler);
    }

    /// <summary>
    /// Represents an asynchronous event handler delegate that can handle events with custom event argument types.
    /// </summary>
    /// <typeparam name="TEventArgs">
    /// The type of the event arguments passed to the event handler.
    /// </typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance of <typeparamref name="TEventArgs"/> containing event data.</param>
    public delegate Task AsyncEventHandlerDelegate<TEventArgs>(object? sender, TEventArgs e);
}