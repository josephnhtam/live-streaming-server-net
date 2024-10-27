using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Hls
{
    /// <summary>
    /// Default implementation of HLS output path resolver.
    /// Creates a path in the format: ./output/{contextIdentifier}/output.m3u8
    /// </summary>
    public class DefaultHlsOutputPathResolver : IHlsOutputPathResolver
    {
        public ValueTask<string> ResolveOutputPath(
           IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString());
            return ValueTask.FromResult(Path.Combine(directory, "output.m3u8"));
        }
    }
}
