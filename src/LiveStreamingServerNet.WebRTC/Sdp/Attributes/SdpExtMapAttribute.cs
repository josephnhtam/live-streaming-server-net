using LiveStreamingServerNet.WebRTC.Sdp.Internal;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    [SdpAttributeName(SdpAttributeNames.ExtMap)]
    public sealed record SdpExtMapAttribute(int Id, string Uri, string? Direction = null, string? ExtensionAttributes = null) : SdpAttributeBase
    {
        public override string Name => SdpAttributeNames.ExtMap;

        public override string Value
        {
            get
            {
                var idPart = Direction == null ? $"{Id}" : $"{Id}/{Direction}";
                return ExtensionAttributes == null
                    ? $"{idPart} {Uri}"
                    : $"{idPart} {Uri} {ExtensionAttributes}";
            }
        }

        public static SdpExtMapAttribute? ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var spaceIndex = value.IndexOf(' ');
            if (spaceIndex < 0)
                return null;

            var idPart = value[..spaceIndex].Trim();
            var rest = value[(spaceIndex + 1)..].Trim();

            int id;
            string? direction = null;

            var slashIndex = idPart.IndexOf('/');
            if (slashIndex < 0)
            {
                if (!int.TryParse(idPart, out id))
                    return null;
            }
            else
            {
                if (!int.TryParse(idPart[..slashIndex], out id))
                    return null;

                direction = idPart[(slashIndex + 1)..];
            }

            string uri;
            string? extensionAttributes = null;

            var restSpaceIndex = rest.IndexOf(' ');
            if (restSpaceIndex < 0)
            {
                uri = rest;
            }
            else
            {
                uri = rest[..restSpaceIndex];
                extensionAttributes = rest[(restSpaceIndex + 1)..];
            }

            return new SdpExtMapAttribute(id, uri, direction, extensionAttributes);
        }
    }
}
