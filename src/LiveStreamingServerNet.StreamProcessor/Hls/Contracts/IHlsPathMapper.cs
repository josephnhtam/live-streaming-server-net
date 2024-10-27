namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    /// <summary>
    /// Defines a contract for mapping stream paths to HLS output paths.
    /// </summary>
    public interface IHlsPathMapper
    {
        /// <summary>
        /// Gets the input stream path to the corresponding HLS output path.
        /// </summary>
        /// <param name="streamPath">The original path of the input stream.</param>
        /// <returns>The mapped HLS output path, or null if no mapping exists.</returns>
        string? GetHlsOutputPath(string streamPath);
    }
}
