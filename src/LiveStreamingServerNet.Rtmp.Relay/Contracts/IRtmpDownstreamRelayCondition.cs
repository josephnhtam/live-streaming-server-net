namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    /// <summary>
    /// Defines conditions for establishing a downstream (pulling a stream from an origin server).
    /// </summary>
    public interface IRtmpDownstreamRelayCondition
    {
        /// <summary>
        /// Determines if a downstream should be established.
        /// </summary>
        /// <param name="services">The service provider for accessing application services</param>
        /// <param name="streamPath">The path of the requested downstream</param>
        /// <returns>
        /// A ValueTask that resolves to true if the downstream should be established, false otherwise
        /// </returns>
        ValueTask<bool> ShouldRelayStreamAsync(IServiceProvider services, string streamPath);
    }
}
