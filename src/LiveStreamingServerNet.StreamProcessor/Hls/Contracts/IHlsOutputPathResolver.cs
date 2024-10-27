namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    /// <summary>
    /// Defines a contract for resolving output file paths for HLS content.
    /// </summary>
    public interface IHlsOutputPathResolver
    {
        /// <summary>
        /// Resolves the output path for HLS content based on the provided context and stream information.
        /// </summary>
        /// <param name="services">The service provider for dependency injection.</param>
        /// <param name="contextIdentifier">Unique identifier for the streaming context.</param>
        /// <param name="streamPath">The original path of the input stream.</param>
        /// <param name="streamArguments">Additional arguments associated with the stream.</param>
        /// <returns>A ValueTask containing the resolved output path as a string.</returns>
        ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
