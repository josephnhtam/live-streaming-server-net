using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.M3u8.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.M3u8
{
    internal class MediaPlaylist : IPlaylist
    {
        public bool IsMaster => false;
        public Manifest Manifest { get; }
        public IReadOnlyList<ManifestTsSegment> TsSegments { get; }

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

        private MediaPlaylist(Manifest content, List<ManifestTsSegment> tsSegments)
        {
            Manifest = content;
            TsSegments = tsSegments;
        }

        public static MediaPlaylist Parse(Manifest content)
        {
            List<ManifestTsSegment> tsSegments = new List<ManifestTsSegment>();

            using var stringReader = new StringReader(content.Content);

            var line = stringReader.ReadLine();
            if (line?.Equals("#EXTM3U") != true)
                throw new InvalidOperationException("Invalid M3U8 file format");

            while ((line = stringReader.ReadLine()) != null)
                if (line.StartsWith("#EXTINF") && (line = stringReader.ReadLine()) != null)
                    tsSegments.Add(new ManifestTsSegment(content.Name, line));

            return new MediaPlaylist(content, new List<ManifestTsSegment>(tsSegments));
        }
    }
}
