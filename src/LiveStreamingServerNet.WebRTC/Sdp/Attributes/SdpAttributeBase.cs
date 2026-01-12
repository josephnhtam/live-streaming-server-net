using LiveStreamingServerNet.WebRTC.Sdp.Attributes.Contracts;

namespace LiveStreamingServerNet.WebRTC.Sdp.Attributes
{
    public abstract record SdpAttributeBase : ISdpAttribute
    {
        public abstract string Name { get; }
        public abstract string? Value { get; }

        public override string ToString()
        {
            var name = Name;
            var value = Value;

            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return string.IsNullOrEmpty(value) ? $"a={Name}" : $"a={name}:{value}";
        }

        public static (string Name, string? Value) ParseNameValue(string attributeValue)
        {
            var idx = attributeValue.IndexOf(':');

            if (idx < 0)
                return (attributeValue.Trim(), null);

            var name = attributeValue[..idx].Trim();
            var value = attributeValue[(idx + 1)..].Trim();
            return (name, value);
        }
    }
}
