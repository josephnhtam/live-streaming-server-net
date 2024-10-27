namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Defines a processor that handles RTMP stream processing operations.
    /// </summary>
    public interface IStreamProcessor
    {
        /// <summary>
        /// Gets the name of the stream processor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the unique identifier for this processor context.
        /// </summary>
        Guid ContextIdentifier { get; }

        /// <summary>
        /// Runs the stream processing operation.
        /// </summary>
        /// <param name="inputPath">The input path of the stream to process</param>
        /// <param name="streamPath">The original stream path</param>
        /// <param name="streamArguments">Additional arguments for stream processing</param>
        /// <param name="onStarted">Callback invoked when processing starts</param>
        /// <param name="onEnded">Callback invoked when processing ends</param>
        /// <param name="cancellation">Token to cancel the processing operation</param>
        Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation);
    }

    /// <summary>
    /// Delegate for handling stream processor start events.
    /// </summary>
    /// <param name="outputPath">The path where the processed stream output is available</param>
    public delegate Task OnStreamProcessorStarted(string outputPath);

    /// <summary>
    /// Delegate for handling stream processor end events.
    /// </summary>
    /// <param name="outputPath">The path where the processed stream output was available</param>
    public delegate Task OnStreamProcessorEnded(string outputPath);

}
