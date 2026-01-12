using LiveStreamingServerNet.Utilities.Extensions;

namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public readonly record struct SdpConnection(string NetworkType, string AddressType, string Address)
    {
        public string Value => $"{NetworkType} {AddressType} {Address}";

        public override string ToString() => $"c={Value}";

        public static SdpConnection ParseValue(string value)
        {
            var parts = value.SplitBySpaces();

            if (parts.Length < 3)
                throw new FormatException("Invalid c= line");

            return new SdpConnection(parts[0], parts[1], parts[2]);
        }
    }
}
