namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsOutputPathResolver
    {
        ValueTask<HlsOutputPath> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }

    public record struct HlsOutputPath
    {
        public string ManifestOutputPath { get; init; }
        public string TsSegmentOutputPath { get; init; }
    }
}
