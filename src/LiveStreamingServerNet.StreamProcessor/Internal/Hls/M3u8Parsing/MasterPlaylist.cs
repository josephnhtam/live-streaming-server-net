using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8Parsing.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8Parsing
{
    internal class MasterPlaylist : IPlaylist
    {
        public bool IsMaster => true;
        public Manifest Manifest { get; }
        public IReadOnlyList<MediaPlaylist> MediaPlaylists { get; }
        public IReadOnlyList<ManifestTsSegment> TsSegments { get; }

        private Dictionary<string, Manifest>? _manifests;
        public IReadOnlyDictionary<string, Manifest> Manifests
        {
            get
            {
                if (_manifests == null)
                    _manifests = MediaPlaylists.Select(x => x.Manifest).ToDictionary(x => x.Name, x => x);

                return _manifests;
            }
        }

        private MasterPlaylist(Manifest content, List<MediaPlaylist> mediaPlaylists)
        {
            Manifest = content;
            MediaPlaylists = mediaPlaylists;
            TsSegments = mediaPlaylists.SelectMany(x => x.TsSegments).Distinct().ToList();
        }

        public static MasterPlaylist Parse(Manifest content, string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath) ?? string.Empty;
            var mediaPlaylists = new List<MediaPlaylist>();

            using var stringReader = new StringReader(content.Content);

            var line = stringReader.ReadLine();
            if (line?.Equals("#EXTM3U") != true)
                throw new InvalidOperationException("Invalid M3U8 file format");

            while ((line = stringReader.ReadLine()) != null)
            {
                if (line.StartsWith("#EXT-X-STREAM-INF") && (line = stringReader.ReadLine()) != null)
                {
                    var subManifestPath = Path.Combine(dirPath, line);
                    var mediaPlaylistContent = File.ReadAllText(subManifestPath);
                    mediaPlaylists.Add(MediaPlaylist.Parse(new(line, mediaPlaylistContent)));
                }
            }

            return new MasterPlaylist(content, mediaPlaylists);
        }
    }
}
