using LiveStreamingServerNet.StreamProcessor.Hls;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Unmarshal.Contracts
{
    internal interface IPlaylist : IManifestContainer, ITsFilesContainer
    {
        Manifest Manifest { get; }
    }

    internal interface IManifestContainer
    {
        IReadOnlyDictionary<string, Manifest> Manifests { get; }
    }

    internal interface ITsFilesContainer
    {
        IReadOnlyList<TsFile> TsFiles { get; }
    }
}
