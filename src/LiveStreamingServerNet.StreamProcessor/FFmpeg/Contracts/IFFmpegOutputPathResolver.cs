namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Contracts
{
    public interface IFFmpegOutputPathResolver
    {
        ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
