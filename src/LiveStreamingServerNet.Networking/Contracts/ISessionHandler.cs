namespace LiveStreamingServerNet.Networking.Contracts
{
    /// <summary>
    /// Interface for handling network session lifecycle and data processing.
    /// </summary>
    public interface ISessionHandler : IAsyncDisposable
    {
        /// <summary>
        /// Performs session initialization tasks before the main session loop begins.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the initialization process.</param>
        /// <returns>
        /// true if initialization was successful and session can proceed.
        /// false if initialization failed and session should be terminated.
        /// </returns>
        ValueTask<bool> InitializeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the main session processing loop, handling incoming data from the network stream.
        /// </summary>
        /// <param name="networkStream">Stream for reading network data.</param>
        /// <param name="cancellationToken">Token to cancel the session loop.</param>
        /// <returns>
        /// true if session can proceed.
        /// false if session should be terminated.
        /// </returns>
        Task<bool> HandleSessionLoopAsync(INetworkStreamReader networkStream, CancellationToken cancellationToken);
    }
}
