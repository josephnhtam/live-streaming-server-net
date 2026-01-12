using LiveStreamingServerNet.Utilities.Extensions;

namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public readonly record struct SdpTiming(string StartTime, string StopTime)
    {
        public string Value => $"{StartTime} {StopTime}";

        public override string ToString() => $"t={Value}";

        public static SdpTiming ParseValue(string value)
        {
            var parts = value.SplitBySpaces();

            if (parts.Length < 2)
                throw new FormatException("Invalid t= line");

            return new SdpTiming(parts[0], parts[1]);
        }
    }
}
