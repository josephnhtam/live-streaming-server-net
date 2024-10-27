namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Resolves input paths for RTMP streams.
    /// </summary>
    public interface IInputPathResolver
    {
        /// <summary>
        /// Resolves the actual input path for a stream based on the stream path and arguments.
        /// </summary>
        /// <param name="streamPath">The original stream path</param>
        /// <param name="streamArguments">Additional arguments provided with the stream</param>
        /// <returns>The resolved input path</returns>
        Task<string> ResolveInputPathAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
