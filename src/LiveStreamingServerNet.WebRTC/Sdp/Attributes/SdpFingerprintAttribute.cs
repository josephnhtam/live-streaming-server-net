using LiveStreamingServerNet.WebRTC.Sdp.Internal;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    [SdpAttributeName(SdpAttributeNames.Fingerprint)]
    public sealed record SdpFingerprintAttribute(string HashFunction, string Fingerprint) : SdpAttributeBase
    {
        public override string Name => SdpAttributeNames.Fingerprint;
        public override string? Value => $"{HashFunction} {Fingerprint}";

        public static SdpFingerprintAttribute? ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var spaceIndex = value.IndexOf(' ');
            if (spaceIndex < 0)
                return null;

            var hashFunction = value[..spaceIndex].Trim();
            var fingerprint = value[(spaceIndex + 1)..].Trim();

            if (string.IsNullOrEmpty(hashFunction) || string.IsNullOrEmpty(fingerprint))
                return null;

            return new SdpFingerprintAttribute(hashFunction, fingerprint);
        }
    }
}
