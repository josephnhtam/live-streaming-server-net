using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Marshal.Contracts
{
    internal interface IManifestWriter
    {
        Task WriteAsync(string ManifestOutputPath, IEnumerable<TsSegment> segments, CancellationToken cancellationToken = default);
    }
}
