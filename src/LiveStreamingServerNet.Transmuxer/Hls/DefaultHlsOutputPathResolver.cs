using LiveStreamingServerNet.Transmuxer.Hls.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Hls
{
    internal class DefaultHlsOutputPathResolver : IHlsOutputPathResolver
    {
        public Task<HlsOutputPath> ResolveOutputPath(
            Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString());

            return Task.FromResult(new HlsOutputPath
            {
                ManifestOutputPath = Path.Combine(directory, "output.m3u8"),
                TsFileOutputPath = Path.Combine(directory, "output{seqNum}.ts")
            });
        }
    }
}
