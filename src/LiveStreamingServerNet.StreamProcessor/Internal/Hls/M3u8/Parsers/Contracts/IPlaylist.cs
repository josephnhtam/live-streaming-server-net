using LiveStreamingServerNet.StreamProcessor.Hls;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Parsers.Contracts
{
    internal interface IPlaylist : IManifestContainer, ISegmentsContainer
    {
        bool IsMaster { get; }
        Manifest Manifest { get; }
    }

    internal interface IManifestContainer
    {
        IReadOnlyDictionary<string, Manifest> Manifests { get; }
    }

    internal interface ISegmentsContainer
    {
        IReadOnlyList<Segment> Segments { get; }
    }
}
