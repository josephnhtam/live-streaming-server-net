namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts
{
    /// <summary>
    /// Resolves output paths for FFmpeg stream processing operations.
    /// </summary>
    public interface IFFmpegOutputPathResolver
    {
        /// <summary>
        /// Determines the output path for FFmpeg processed streams.
        /// </summary>
        /// <param name="services">Service provider for accessing dependencies</param>
        /// <param name="contextIdentifier">Unique identifier for the processing context</param>
        /// <param name="streamPath">Original path of the stream being processed</param>
        /// <param name="streamArguments">Additional arguments associated with the stream</param>
        /// <returns>The resolved output path where FFmpeg should write processed content</returns>
        ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
