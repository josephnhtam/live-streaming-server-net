using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.M3u8
{
    internal class MasterPlaylist : IPlaylist
    {
        public Manifest Manifest { get; }
        public IReadOnlyList<MediaPlaylist> MediaPlaylists { get; }
        public IReadOnlyList<TsFile> TsFiles { get; }

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
            TsFiles = mediaPlaylists.SelectMany(x => x.TsFiles).Distinct().ToList();
        }

        public static MasterPlaylist Parse(Manifest content)
        {
            List<MediaPlaylist> mediaPlaylists = new List<MediaPlaylist>();

            using var stringReader = new StringReader(content.Content);

            var line = stringReader.ReadLine();
            if (line?.Equals("#EXTM3U") != true)
                throw new InvalidOperationException("Invalid M3U8 file format");

            while ((line = stringReader.ReadLine()) != null)
            {
                if (line.StartsWith("#EXT-X-STREAM-INF") && (line = stringReader.ReadLine()) != null)
                {
                    var mediaPlaylistContent = File.ReadAllText(line);
                    mediaPlaylists.Add(MediaPlaylist.Parse(new(line, mediaPlaylistContent)));
                }
            }

            return new MasterPlaylist(content, mediaPlaylists);
        }
    }
}
