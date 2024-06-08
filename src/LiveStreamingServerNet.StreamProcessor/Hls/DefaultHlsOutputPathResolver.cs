using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Hls
{
    internal class DefaultHlsOutputPathResolver : IHlsOutputPathResolver
    {
        public ValueTask<HlsOutputPath> ResolveOutputPath(
           IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "output", contextIdentifier.ToString());

            return ValueTask.FromResult(new HlsOutputPath
            {
                ManifestOutputPath = Path.Combine(directory, "output.m3u8"),
                TsSegmentOutputPath = Path.Combine(directory, "output{seqNum}.ts")
            });
        }
    }
}
