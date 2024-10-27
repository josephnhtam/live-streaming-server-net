namespace LiveStreamingServerNet.Networking.Client.Contracts
{
    /// <summary>
    /// Interface representing client for lifecycle management.
    /// </summary>
    public interface IClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets service provider for dependency resolution.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Starts client and waits until client stops.
        /// </summary>
        /// <param name="serverEndPoint">Target endpoint for connection.</param>
        /// <param name="cancellationToken">Token to stop client.</param>
        /// <returns>Task completing when client shuts down.</returns>
        Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default);
    }
}
