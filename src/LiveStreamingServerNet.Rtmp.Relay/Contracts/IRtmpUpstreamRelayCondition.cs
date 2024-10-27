namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    /// <summary>
    /// Defines conditions for establishing an upstream (pushing a stream to an origin server).
    /// </summary>
    public interface IRtmpUpstreamRelayCondition
    {
        /// <summary>
        /// Determines if an upstream should be established.
        /// </summary>
        /// <param name="services">The service provider for accessing application services</param>
        /// <param name="streamPath">The path of the proposed upstream</param>
        /// <param name="streamArguments">Additional arguments provided with the publish request</param>
        /// <returns>
        /// A ValueTask that resolves to true if the upstream should be established, false otherwise
        /// </returns>
        ValueTask<bool> ShouldRelayStreamAsync(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
