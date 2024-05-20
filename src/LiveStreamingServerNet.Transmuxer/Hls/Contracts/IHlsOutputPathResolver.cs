namespace LiveStreamingServerNet.Transmuxer.Hls.Contracts
{
    public interface IHlsOutputPathResolver
    {
        Task<HlsOutputPath> ResolveOutputPath(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }

    public record struct HlsOutputPath
    {
        public string ManifestOutputPath { get; init; }
        public string TsFileOutputPath { get; init; }
    }
}
