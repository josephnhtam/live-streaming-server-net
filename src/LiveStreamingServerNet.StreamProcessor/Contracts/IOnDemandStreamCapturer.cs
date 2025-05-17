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
        /// Captures a segment of the stream with the specified duration and frame rate.
        /// </summary>
        /// <param name="streamPath">The URL or file path of the stream.</param>
        /// <param name="streamArguments">Additional arguments to pass to the stream.</param>
        /// <param name="outputPath">The file path where the captured segment will be saved.</param>
        /// <param name="duration">The duration of the segment to capture.</param>
        /// <param name="options">The options for capturing the segment.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A task representing the asynchronous capture operation.</returns>
        Task CaptureSegmentAsync(
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string outputPath,
            TimeSpan duration,
            SegmentCaptureOptions? options,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents options for capturing a segment of a stream.
    /// /// </summary>
    /// <param name="Height">Optional parameter that specifies the height.</param>
    /// <param name="Framerate">The frame rate for the captured segment.</param>
    /// <param name="AudioFrequency">Optional parameter that specifies the audio frequency.</param>
    /// <param name="AudioChannels">Optional parameter that specifies the number of audio channels.</param>
    public record SegmentCaptureOptions(int? Height = null, int? Framerate = null, int? AudioFrequency = null, int? AudioChannels = null);
}
