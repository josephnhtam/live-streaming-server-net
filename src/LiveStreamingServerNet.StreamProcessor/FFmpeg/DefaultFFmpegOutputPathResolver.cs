using LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.FFmpeg
{
    public class DefaultFFmpegOutputPathResolver : IFFmpegOutputPathResolver
    {
        public ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.FromResult(Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString(), "output.m3u8"));
        }
    }
}
