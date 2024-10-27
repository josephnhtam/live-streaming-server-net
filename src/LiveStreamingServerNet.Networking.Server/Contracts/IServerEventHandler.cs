using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Contracts
{
    /// <summary>
    /// Handles server lifecycle and client connection events.
    /// </summary>
    public interface IServerEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Called when TCP listener is created for an endpoint.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        /// <param name="tcpListener">Created TCP listener instance.</param>
        Task OnListenerCreatedAsync(IEventContext context, ITcpListener tcpListener);

        /// <summary>
        /// Called when TCP client connection is accepted.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        /// <param name="tcpClient">Accepted TCP client connection.</param>
        Task OnClientAcceptedAsync(IEventContext context, ITcpClient tcpClient);

        /// <summary>
        /// Called when client session is established.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        /// <param name="client">Handle to connected client session.</param>
        Task OnClientConnectedAsync(IEventContext context, ISessionHandle client);

        /// <summary>
        /// Called when client session is terminated.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        /// <param name="client">Information about disconnected client.</param>
        Task OnClientDisconnectedAsync(IEventContext context, ISessionInfo client);

        /// <summary>
        /// Called when server starts listening.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        Task OnServerStartedAsync(IEventContext context);

        /// <summary>
        /// Called when server stops listening.
        /// </summary>
        /// <param name="context">Event context containing shared state.</param>
        Task OnServerStoppedAsync(IEventContext context);
    }
}
