namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Defines conditions for enabling stream processors.
    /// </summary>
    public interface IStreamProcessorCondition
    {
        /// <summary>
        /// Determines whether a stream processor should be enabled for the given stream.
        /// </summary>
        /// <param name="services">Service provider for accessing dependencies</param>
        /// <param name="streamPath">The path of the stream to evaluate</param>
        /// <param name="streamArguments">Additional arguments associated with the stream</param>
        /// <returns>True if the processor should be enabled, false otherwise</returns>
        ValueTask<bool> IsEnabled(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
