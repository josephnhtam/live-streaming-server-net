using LiveStreamingServerNet.Transmuxer.Hls;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Contracts
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
