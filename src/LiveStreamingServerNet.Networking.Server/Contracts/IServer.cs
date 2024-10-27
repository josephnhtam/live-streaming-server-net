using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Server.Contracts
{
    /// <summary>
    /// Interface representing server for lifecycle management.
    /// </summary>
    public interface IServer : IServerHandle
    {
        /// <summary>
        /// Gets service provider for dependency resolution.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Starts server on specified endpoint and waits until server stops.
        /// </summary>
        /// <param name="serverEndPoint">Target endpoint to listen on.</param>
        /// <param name="cancellationToken">Token to stop server.</param>
        /// <returns>Task completing when server shuts down.</returns>
        Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts server on multiple endpoints and waits until server stops.
        /// </summary>
        /// <param name="serverEndPoints">List of endpoints to listen on.</param>
        /// <param name="cancellationToken">Token to stop server.</param>
        /// <returns>Task completing when server shuts down.</returns>
        Task RunAsync(IReadOnlyList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface providing server state and client access.
    /// </summary>
    public interface IServerHandle
    {
        /// <summary>
        /// Gets whether server is currently running.
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Gets list of endpoints server is listening on.
        /// Null if server not started.
        /// </summary>
        IReadOnlyList<ServerEndPoint>? EndPoints { get; }

        /// <summary>
        /// Gets list of connected client sessions.
        /// </summary>
        IReadOnlyList<ISessionHandle> Clients { get; }

        /// <summary>
        /// Gets client session by ID.
        /// </summary>
        /// <param name="clientId">ID of client to retrieve.</param>
        /// <returns>Client session if found, null otherwise.</returns>
        ISessionHandle? GetClient(uint clientId);
    }
}
