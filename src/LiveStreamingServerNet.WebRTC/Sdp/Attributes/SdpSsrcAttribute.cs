using LiveStreamingServerNet.WebRTC.Sdp.Internal;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    [SdpAttributeName(SdpAttributeNames.Ssrc)]
    public sealed record SdpSsrcAttribute(uint Ssrc, string Attribute, string? AttributeValue = null) : SdpAttributeBase
    {
        public override string Name => SdpAttributeNames.Ssrc;

        public override string? Value => string.IsNullOrEmpty(AttributeValue)
            ? $"{Ssrc} {Attribute}"
            : $"{Ssrc} {Attribute}:{AttributeValue}";

        public static SdpSsrcAttribute? ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var spaceIndex = value.IndexOf(' ');
            if (spaceIndex < 0)
                return null;

            var ssrcStr = value[..spaceIndex].Trim();
            if (!uint.TryParse(ssrcStr, out var ssrc))
                return null;

            var rest = value[(spaceIndex + 1)..].Trim();
            if (string.IsNullOrEmpty(rest))
                return null;

            var colonIndex = rest.IndexOf(':');
            string attribute;
            string? attributeValue;

            if (colonIndex < 0)
            {
                attribute = rest;
                attributeValue = null;
            }
            else
            {
                attribute = rest[..colonIndex].Trim();
                attributeValue = rest[(colonIndex + 1)..];
            }

            return new SdpSsrcAttribute(ssrc, attribute, attributeValue);
        }
    }
}
