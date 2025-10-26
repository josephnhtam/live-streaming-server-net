using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Defines a contract for resolving FFmpeg process configurations based on stream context.
    /// </summary>
    public interface IFFmpegProcessConfigurationResolver
    {
        /// <summary>
        /// Resolves the FFmpeg process configuration based on the provided stream path and arguments.
        /// </summary>
        /// <param name="services">Service provider for accessing dependencies</param>
        /// <param name="contextIdentifier">Unique identifier for the processing context</param>
        /// <param name="streamPath">Original path of the stream being processed</param>
        /// <param name="streamArguments">Additional arguments associated with the stream</param>
        /// <returns>The resolved FFmpeg process configuration, or null to use the default configuration</returns>
        ValueTask<FFmpegProcessConfiguration?> ResolveAsync(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
