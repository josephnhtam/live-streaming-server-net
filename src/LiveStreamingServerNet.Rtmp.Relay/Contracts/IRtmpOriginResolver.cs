namespace LiveStreamingServerNet.Rtmp.Relay.Contracts
{
    /// <summary>
    /// Resolves the origin server details for RTMP streams in both upstream and downstream scenarios.
    /// Used to determine which RTMP server should be connected to for stream relay.
    /// </summary>
    public interface IRtmpOriginResolver
    {
        /// <summary>
        /// Resolves the origin server for publishing (pushing).
        /// </summary>
        /// <param name="streamPath">The path of the stream being published</param>
        /// <param name="streamArguments">Additional arguments associated with the stream request</param>
        /// <param name="cancellationToken">Token to cancel the resolution operation</param>
        /// <returns>The origin server details if resolved, null if no suitable origin is found</returns>
        ValueTask<RtmpOrigin?> ResolveUpstreamOriginAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments, CancellationToken cancellationToken);

        /// <summary>
        /// Resolves the origin server for playing (pulling).
        /// </summary>
        /// <param name="streamPath">The path of the stream being requested</param>
        /// <param name="cancellationToken">Token to cancel the resolution operation</param>
        /// <returns>The origin server details if resolved, null if no suitable origin is found</returns>
        ValueTask<RtmpOrigin?> ResolveDownstreamOriginAsync(string streamPath, CancellationToken cancellationToken);
    }
}
