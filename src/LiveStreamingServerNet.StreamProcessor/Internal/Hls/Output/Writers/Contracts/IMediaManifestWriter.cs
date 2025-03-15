using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts
{
    internal interface IMediaManifestWriter
    {
        Task WriteAsync(string ManifestOutputPath, IEnumerable<TsSegment> segments, CancellationToken cancellationToken = default);
    }
}
