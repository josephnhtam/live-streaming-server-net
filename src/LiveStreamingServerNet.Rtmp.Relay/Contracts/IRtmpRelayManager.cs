namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    /// <summary>
    /// Manages RTMP relays by handling downstream requests and tracking their states.
    /// </summary>
    public interface IRtmpRelayManager
    {
        /// <summary>
        /// Requests a new downstream from an origin server.
        /// </summary>
        /// <param name="streamPath">The path of the desired downstream</param>
        /// <param name="cancellationToken">Token to cancel the request operation</param>
        /// <returns>
        /// A subscriber object if the downstream is established, null if the request cannot be fulfilled
        /// </returns>
        Task<IRtmpDownstreamSubscriber?> RequestDownstreamAsync(string streamPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a downstream is currently active for the specified path.
        /// </summary>
        /// <param name="streamPath">The path to check for an active downstream</param>
        /// <returns>True if a downstream exists at the specified path, false otherwise</returns>
        bool IsDownstreamRequested(string streamPath);
    }
}
