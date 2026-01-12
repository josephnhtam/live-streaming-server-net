using LiveStreamingServerNet.Utilities.Extensions;

namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public readonly record struct SdpOrigin(
        string Username,
        string SessionId,
        string SessionVersion,
        string NetworkType,
        string AddressType,
        string UnicastAddress)
    {
        public string Value =>
            $"{Username} {SessionId} {SessionVersion} {NetworkType} {AddressType} {UnicastAddress}";

        public override string ToString() => $"o={Value}";

        public static SdpOrigin ParseValue(string value)
        {
            var parts = value.SplitBySpaces();

            if (parts.Length < 6)
                throw new FormatException("Invalid o= line");

            return new SdpOrigin(
                Username: parts[0],
                SessionId: parts[1],
                SessionVersion: parts[2],
                NetworkType: parts[3],
                AddressType: parts[4],
                UnicastAddress: parts[5]
            );
        }
    }
}
