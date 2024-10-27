using LiveStreamingServerNet.Networking;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    /// <summary>
    /// Represents a client for RTMP communications.
    /// </summary>
    public interface IRtmpClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the service provider for dependency resolution.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Gets the current status of the RTMP client connection.
        /// </summary>
        RtmpClientStatus Status { get; }

        /// <summary>
        /// Gets the current bandwidth limits imposed by the server.
        /// </summary>
        RtmpBandwidthLimit? BandwidthLimit { get; }

        /// <summary>
        /// Connects to an RTMP server.
        /// </summary>
        /// <param name="endPoint">Server endpoint to connect to</param>
        /// <param name="appName">Name of the application to connect to</param>
        /// <returns>Server's response to the connect request</returns>
        Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName);

        /// <summary>
        /// Connects to an RTMP server.
        /// </summary>
        /// <param name="endPoint">Server endpoint to connect to</param>
        /// <param name="appName">Name of the application to connect to</param>
        /// <param name="information">Additional connection parameters</param>
        /// <returns>Server's response to the connect request</returns>
        Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName, IDictionary<string, object> information);

        /// <summary>
        /// Creates a new RTMP stream for publishing or playing media.
        /// </summary>
        /// <returns>A new RTMP stream instance</returns>
        Task<IRtmpStream> CreateStreamAsync();

        /// <summary>
        /// Enqueues sending a command to the server.
        /// </summary>
        /// <param name="command">Command to send</param>
        void Command(RtmpCommand command);

        /// <summary>
        /// Enqueues sending a command to the server and waits for response.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>Server's response to the command</returns>
        Task<RtmpCommandResponse> CommandAsync(RtmpCommand command);

        /// <summary>
        /// Waits until the client is stopped or the cancellation token is triggered.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the wait operation</param>
        Task UntilStoppedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the RTMP client and closes the connection.
        /// </summary>
        void Stop();

        /// <summary>
        /// Event raised when server updates the bandwidth limits.
        /// </summary>
        event EventHandler<BandwidthLimitEventArgs> OnBandwidthLimitUpdated;
    }

    /// <summary>
    /// Event arguments for bandwidth limit updates from server.
    /// </summary>
    /// <param name="BandwidthLimit">New bandwidth limits or null if limits are removed</param>
    public record struct BandwidthLimitEventArgs(RtmpBandwidthLimit? BandwidthLimit);

    /// <summary>
    /// Response from server after connect request.
    /// </summary>
    /// <param name="CommandObject">Properties returned by server about the connection</param>
    /// <param name="Parameters">Optional additional parameters from server</param>
    public record struct ConnectResponse(IReadOnlyDictionary<string, object> CommandObject, object? Parameters);
}
