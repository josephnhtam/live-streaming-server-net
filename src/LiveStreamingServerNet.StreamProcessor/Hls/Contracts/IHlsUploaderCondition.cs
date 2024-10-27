namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    /// <summary>
    /// Defines a contract for determining whether HLS content should be uploaded.
    /// </summary>
    public interface IHlsUploaderCondition
    {
        /// <summary>
        /// Determines whether HLS content should be uploaded based on the given context.
        /// </summary>
        /// <param name="context">The context containing information about the stream being processed.</param>
        /// <returns>A ValueTask containing true if content should be uploaded, false otherwise.</returns>
        ValueTask<bool> ShouldUploadAsync(StreamProcessingContext context);
    }
}
