namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Represents a service that captures streams on demand.
    /// </summary>
    public interface IOnDemandStreamCapturer
    {
        /// <summary>
        /// Captures a snapshot from the specified stream.
        /// </summary>
        /// <param name="streamPath">The URL or file path of the stream.</param>
        /// <param name="streamArguments">Additional arguments to pass to the stream.</param>
        /// <param name="outputPath">The file path where the snapshot will be saved.</param>
        /// <param name="height">Optional parameter that specifies the height.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A task representing the asynchronous capture operation.</returns>
        Task CaptureSnapshotAsync(
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string outputPath,
            int? height,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Captures a clip of the stream with the specified duration and frame rate.
        /// </summary>
        /// <param name="streamPath">The URL or file path of the stream.</param>
        /// <param name="streamArguments">Additional arguments to pass to the stream.</param>
        /// <param name="outputPath">The file path where the captured clip will be saved.</param>
        /// <param name="options">The options for capturing the clip.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A task representing the asynchronous capture operation.</returns>
        Task CaptureClipAsync(
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string outputPath,
            ClipCaptureOptions options,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents options for capturing a clip of a stream.
    /// /// </summary>
    /// <param name="Duration">The duration of the clips to capture.</param>
    /// <param name="Height">Optional parameter that specifies the height.</param>
    /// <param name="Framerate">The frame rate for the captured clip.</param>
    /// <param name="AudioFrequency">Optional parameter that specifies the audio frequency.</param>
    /// <param name="AudioChannels">Optional parameter that specifies the number of audio channels.</param>
    public record ClipCaptureOptions(TimeSpan Duration, int? Height = null, int? Framerate = null, int? AudioFrequency = null, int? AudioChannels = null);
}
