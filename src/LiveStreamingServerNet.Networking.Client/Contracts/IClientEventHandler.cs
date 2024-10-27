using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Client.Contracts
{
    /// <summary>
    /// Handles client lifecycle events.
    /// </summary>
    public interface IClientEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Called when client establishes connection with server.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        /// <param name="session">Handle to connected client session.</param>
        Task OnClientConnectedAsync(IEventContext context, ISessionHandle session);

        /// <summary>
        /// Called when client disconnects or stops.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        Task OnClientStoppedAsync(IEventContext context);
    }
}
