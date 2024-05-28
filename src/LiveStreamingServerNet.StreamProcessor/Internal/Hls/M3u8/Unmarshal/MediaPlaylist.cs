using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Unmarshal.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Unmarshal
{
    internal class MediaPlaylist : IPlaylist
    {
        public Manifest Manifest { get; }
        public IReadOnlyList<TsFile> TsFiles { get; }

        private Dictionary<string, Manifest>? _manifests;
        public IReadOnlyDictionary<string, Manifest> Manifests
        {
            get
            {
                if (_manifests == null)
                    _manifests = new Dictionary<string, Manifest> { [Manifest.Name] = Manifest };

                return _manifests;
            }
        }

        private MediaPlaylist(Manifest content, List<TsFile> tsFiles)
        {
            Manifest = content;
            TsFiles = tsFiles;
        }

        public static MediaPlaylist Parse(Manifest content)
        {
            List<TsFile> tsFiles = new List<TsFile>();

            using var stringReader = new StringReader(content.Content);

            var line = stringReader.ReadLine();
            if (line?.Equals("#EXTM3U") != true)
                throw new InvalidOperationException("Invalid M3U8 file format");

            while ((line = stringReader.ReadLine()) != null)
                if (line.StartsWith("#EXTINF") && (line = stringReader.ReadLine()) != null)
                    tsFiles.Add(new TsFile(content.Name, line));

            return new MediaPlaylist(content, new List<TsFile>(tsFiles));
        }
    }
}
