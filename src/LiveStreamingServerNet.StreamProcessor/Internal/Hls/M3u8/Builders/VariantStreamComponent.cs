using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts;
using System.Text;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal class VariantStreamComponent : IManifestComponent
    {
        private readonly List<VariantStream> _variantStreams;

        public VariantStreamComponent(List<VariantStream> variantStreams)
        {
            _variantStreams = variantStreams;
        }

        public string Build()
        {
            var sb = new StringBuilder();

            foreach (var variant in _variantStreams)
            {
                var attributes = new List<string>()
                {
                    $"NAME=\"{variant.Name}\""
                };

                if (variant.Bandwidth.HasValue && variant.Bandwidth.Value > 0)
                {
                    attributes.Add($"BANDWIDTH={variant.Bandwidth.Value}");
                }

                if (!string.IsNullOrWhiteSpace(variant.Resolution))
                    attributes.Add($"RESOLUTION={variant.Resolution}");

                if (!string.IsNullOrWhiteSpace(variant.Codecs))
                    attributes.Add($"CODECS=\"{variant.Codecs}\"");

                if (!string.IsNullOrWhiteSpace(variant.Language))
                    attributes.Add($"LANGUAGE=\"{variant.Language}\"");

                if (!string.IsNullOrWhiteSpace(variant.GroupId))
                    attributes.Add($"GROUP-ID=\"{variant.GroupId}\"");

                if (variant.ExtraAttributes != null)
                {
                    foreach (var kvp in variant.ExtraAttributes)
                    {
                        attributes.Add($"{kvp.Key}=\"{kvp.Value}\"");
                    }
                }

                sb.AppendLine($"#EXT-X-STREAM-INF:{string.Join(",", attributes)}");
                sb.AppendLine(variant.Uri);
            }

            return sb.ToString();
        }
    }
}
