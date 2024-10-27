using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Handles events related to stream processor lifecycle.
    /// </summary>
    public interface IStreamProcessorEventHandler
    {
        /// <summary>
        /// Gets the execution order of this handler. Lower numbers execute first.
        /// </summary>
        /// <returns>The order value, default is 0</returns>
        int GetOrder() => 0;

        /// <summary>
        /// Handles stream processor start events.
        /// </summary>
        /// <param name="context">Event context</param>
        /// <param name="processor">Name of the processor</param>
        /// <param name="identifier">Unique identifier of the processor instance</param>
        /// <param name="clientId">ID of the client that initiated the stream</param>
        /// <param name="inputPath">Path of the input stream</param>
        /// <param name="outputPath">Path where processed output is available</param>
        /// <param name="streamPath">Original stream path</param>
        /// <param name="streamArguments">Additional stream arguments</param>
        Task OnStreamProcessorStartedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);

        /// <summary>
        /// Handles stream processor stop events.
        /// </summary>
        /// <param name="context">Event context</param>
        /// <param name="processor">Name of the processor</param>
        /// <param name="identifier">Unique identifier of the processor instance</param>
        /// <param name="clientId">ID of the client that initiated the stream</param>
        /// <param name="inputPath">Path of the input stream</param>
        /// <param name="outputPath">Path where processed output was available</param>
        /// <param name="streamPath">Original stream path</param>
        /// <param name="streamArguments">Additional stream arguments</param>
        Task OnStreamProcessorStoppedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
