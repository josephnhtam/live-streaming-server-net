using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Parsers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Parsers
{
    internal class MasterPlaylist : IPlaylist
    {
        public bool IsMaster => true;
        public Manifest Manifest { get; }
        public IReadOnlyList<MediaPlaylist> MediaPlaylists { get; }
        public IReadOnlyList<Segment> Segments { get; }

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
            Segments = CollectSegments(mediaPlaylists);
        }

        private List<Segment> CollectSegments(List<MediaPlaylist> mediaPlaylists)
        {
            return mediaPlaylists
                .Select(mediaPlaylist => (mediaPlaylist, dirPath: Path.GetDirectoryName(mediaPlaylist.Manifest.Name)))
                .SelectMany(x =>
                  x.mediaPlaylist.Segments.Select(segment =>
                      segment with { FileName = NormalizePath(Path.Combine(x.dirPath ?? string.Empty, segment.FileName)) }))
                .Distinct()
                .ToList();
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
                if (line.StartsWith("#EXT-X-MEDIA:") && line.Contains("TYPE=SUBTITLES"))
                {
                    var attributes = ParseAttributes(line);

                    if (attributes.TryGetValue("URI", out var uri))
                    {
                        var subtitleManifestPath = Path.Combine(dirPath, uri);
                        var subtitleContent = File.ReadAllText(subtitleManifestPath);
                        mediaPlaylists.Add(MediaPlaylist.Parse(new Manifest(subtitleManifestPath, subtitleContent)));
                    }
                }
                else if (line.StartsWith("#EXT-X-STREAM-INF") && (line = stringReader.ReadLine()) != null)
                {
                    var subManifestPath = Path.Combine(dirPath, line);
                    var mediaPlaylistContent = File.ReadAllText(subManifestPath);
                    mediaPlaylists.Add(MediaPlaylist.Parse(new(line, mediaPlaylistContent)));
                }
            }

            return new MasterPlaylist(content, mediaPlaylists);
        }

        private static Dictionary<string, string> ParseAttributes(string line)
        {
            var attributes = new Dictionary<string, string>();

            int colonIndex = line.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < line.Length - 1)
            {
                var attributesPart = line.Substring(colonIndex + 1);
                var pairs = attributesPart.Split(',');

                foreach (var pair in pairs)
                {
                    var kv = pair.Split('=', 2);

                    if (kv.Length == 2)
                    {
                        attributes[kv[0].Trim()] = kv[1].Trim().Trim('"');
                    }
                }
            }

            return attributes;
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
