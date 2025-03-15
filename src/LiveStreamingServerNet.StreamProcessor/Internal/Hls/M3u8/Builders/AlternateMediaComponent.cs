using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal class AlternateMediaComponent : IManifestComponent
    {
        private readonly List<AlternateMedia> _alternateMedia;

        public AlternateMediaComponent(List<AlternateMedia> alternateMedia)
        {
            _alternateMedia = alternateMedia;
        }

        public string Build()
        {
            var sb = new StringBuilder();

            foreach (var media in _alternateMedia)
            {
                string defaultStr = media.IsDefault ? "YES" : "NO";
                string autoSelectStr = media.AutoSelect ? "YES" : "NO";

                var mediaLine = new StringBuilder();
                mediaLine.Append($"#EXT-X-MEDIA:TYPE={media.Type},GROUP-ID=\"{media.GroupId}\",NAME=\"{media.Name}\"");

                if (!string.IsNullOrWhiteSpace(media.Language))
                {
                    mediaLine.Append($",LANGUAGE=\"{media.Language}\"");
                }

                mediaLine.Append($",DEFAULT={defaultStr},AUTOSELECT={autoSelectStr},URI=\"{media.Uri}\"");
                sb.AppendLine(mediaLine.ToString());
            }

            return sb.ToString();
        }
    }
}
