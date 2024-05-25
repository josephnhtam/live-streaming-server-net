using LiveStreamingServerNet.Transmuxer.Internal.Containers;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Marshal.Contracts
{
    internal interface IManifestWriter
    {
        Task WriteAsync(string ManifestOutputPath, IEnumerable<TsSegment> segments, CancellationToken cancellationToken = default);
    }
}
