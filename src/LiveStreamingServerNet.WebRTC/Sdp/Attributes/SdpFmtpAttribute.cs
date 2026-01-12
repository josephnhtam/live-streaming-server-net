using LiveStreamingServerNet.WebRTC.Sdp.Internal;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    [SdpAttributeName(SdpAttributeNames.Fmtp)]
    public sealed record SdpFmtpAttribute(int Format, IReadOnlyDictionary<string, string> Parameters) : SdpAttributeBase
    {
        public override string Name => SdpAttributeNames.Fmtp;
        public override string? Value => $"{Format} {Parameters}";

        public static SdpFmtpAttribute? ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var spaceIndex = value.IndexOf(' ');
            if (spaceIndex < 0)
                return null;

            var formatStr = value[..spaceIndex].Trim();
            if (!int.TryParse(formatStr, out var format))
                return null;

            var parametersStr = value[(spaceIndex + 1)..].Trim();

            return new SdpFmtpAttribute(format, ParseParameters(parametersStr));
        }

        private static Dictionary<string, string> ParseParameters(string parameters)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(parameters))
                return result;

            var pairs = parameters.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var pair in pairs)
            {
                var equalsIndex = pair.IndexOf('=');

                if (equalsIndex < 0)
                {
                    result[pair] = string.Empty;
                }
                else
                {
                    var key = pair[..equalsIndex].Trim();
                    var val = pair[(equalsIndex + 1)..].Trim();
                    result[key] = val;
                }
            }

            return result;
        }
    }
}
