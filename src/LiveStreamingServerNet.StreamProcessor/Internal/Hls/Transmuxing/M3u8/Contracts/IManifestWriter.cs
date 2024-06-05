using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.M3u8.Contracts
{
    internal interface IManifestWriter
    {
        Task WriteAsync(string ManifestOutputPath, IEnumerable<TsSegment> segments, CancellationToken cancellationToken = default);
    }
}
