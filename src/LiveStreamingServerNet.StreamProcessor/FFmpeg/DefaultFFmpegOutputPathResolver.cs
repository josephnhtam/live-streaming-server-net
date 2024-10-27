using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg
{
    /// <summary>
    /// Default implementation of FFmpeg output path resolver.
    /// Creates a path in the format: ./output/{contextIdentifier}/output.m3u8
    /// </summary>
    public class DefaultFFmpegOutputPathResolver : IFFmpegOutputPathResolver
    {
        public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString(), "output.m3u8"));
        }
    }
}
