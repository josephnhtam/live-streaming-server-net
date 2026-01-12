namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    public sealed record SdpAttribute(string Name, string? Value = null) : SdpAttributeBase
    {
        public override string Name { get; } = Name;
        public override string? Value { get; } = Value;

        public static SdpAttribute ParseValue(string name, string? value) => new(name, value);
    }
}
