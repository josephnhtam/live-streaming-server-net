using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Creates stream processor instances based on client session and stream parameters.
    /// </summary>
    public interface IStreamProcessorFactory
    {
        /// <summary>
        /// Creates a stream processor for the given client session and stream.
        /// </summary>
        /// <param name="client">The client session handle</param>
        /// <param name="contextIdentifier">Unique identifier for the processor context</param>
        /// <param name="streamPath">Path of the stream to process</param>
        /// <param name="streamArguments">Additional arguments for stream processing</param>
        /// <returns>A stream processor instance, or null if no processor should be created</returns>
        Task<IStreamProcessor?> CreateAsync(ISessionHandle client, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
