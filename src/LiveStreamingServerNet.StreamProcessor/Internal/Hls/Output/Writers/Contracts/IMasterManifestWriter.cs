using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts
{
    internal interface IMasterManifestWriter
    {
        Task WriteAsync(string manifestOutputPath, IEnumerable<VariantStream> variantStreams, IEnumerable<AlternateMedia> alternateMedia, CancellationToken cancellationToken = default);
    }
}
