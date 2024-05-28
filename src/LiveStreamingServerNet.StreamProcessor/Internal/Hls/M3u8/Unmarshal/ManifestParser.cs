using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Unmarshal.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Unmarshal
{
    internal static class ManifestParser
    {
        public static IPlaylist Parse(string filePath)
        {
            var name = Path.GetFileName(filePath);
            var content = File.ReadAllText(filePath);

            return IsMasterPlaylist(content)
                ? MasterPlaylist.Parse(new(name, content))
                : MediaPlaylist.Parse(new(name, content));
        }

        private static bool IsMasterPlaylist(string content)
        {
            using var stringReader = new StringReader(content);

            string? line;
            while ((line = stringReader.ReadLine()) != null)
                if (line.StartsWith("#EXT-X-STREAM-INF"))
                    return true;

            return false;
        }
    }
}
