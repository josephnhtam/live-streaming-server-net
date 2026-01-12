using LiveStreamingServerNet.Utilities.Extensions;
using LiveStreamingServerNet.WebRTC.Sdp.Internal;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    [SdpAttributeName(SdpAttributeNames.RtpMap)]
    public sealed record SdpRtpMapAttribute(int PayloadType, string EncodingName, int ClockRate, string? EncodingParameters = null) : SdpAttributeBase
    {
        public override string Name => SdpAttributeNames.RtpMap;

        public override string? Value
        {
            get
            {
                var result = $"{PayloadType} {EncodingName}/{ClockRate}";

                if (!string.IsNullOrWhiteSpace(EncodingParameters))
                    result += $"/{EncodingParameters}";

                return result;
            }
        }

        public static SdpRtpMapAttribute? ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var parts = value.SplitBySpaces();
            if (parts.Length < 2)
                return null;

            if (!int.TryParse(parts[0], out var payloadType))
                return null;

            var codecParts = parts[1].Split('/');
            if (codecParts.Length < 2)
                return null;

            var encodingName = codecParts[0];

            if (!int.TryParse(codecParts[1], out var clockRate))
                return null;

            string? encodingParameters = codecParts.Length > 2 ? codecParts[2] : null;

            return new SdpRtpMapAttribute(payloadType, encodingName, clockRate, encodingParameters);
        }
    }
}
