using LiveStreamingServerNet.StreamProcessor.Internal.Containers;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts
{
    internal interface IMediaManifestWriter
    {
        Task WriteAsync(string manifestOutputPath, IEnumerable<Segment> segments, CancellationToken cancellationToken = default);
    }
}
