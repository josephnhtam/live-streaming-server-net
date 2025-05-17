using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal class MasterManifestBuilder : IMasterManifestBuilder
    {
        private readonly List<VariantStream> _variantStreams = new();
        private readonly List<AlternateMedia> _alternateMedia = new();

        public IMasterManifestBuilder AddVariantStream(VariantStream variant)
        {
            _variantStreams.Add(variant);
            return this;
        }

        public IMasterManifestBuilder AddAlternateMedia(AlternateMedia media)
        {
            _alternateMedia.Add(media);
            return this;
        }

        public string Build()
        {
            var sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");
            sb.AppendLine($"#EXT-X-VERSION:3");

            var alternateMediaComponent = new AlternateMediaComponent(_alternateMedia);
            sb.Append(alternateMediaComponent.Build());

            var variantStreamComponent = new VariantStreamComponent(_variantStreams);
            sb.Append(variantStreamComponent.Build());

            return sb.ToString();
        }
    }
}
