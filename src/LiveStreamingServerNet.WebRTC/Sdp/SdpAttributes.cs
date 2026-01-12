using LiveStreamingServerNet.WebRTC.Sdp.Attributes.Contracts;
using LiveStreamingServerNet.WebRTC.Sdp.Contracts;

namespace LiveStreamingServerNet.WebRTC.Sdp
{
    public class SdpAttributes : List<ISdpAttribute>, IReadOnlySdpAttributes
    {
        public string? GetAttributeValue(string name)
        {
            for (var i = 0; i < Count; i++)
            {
                var attribute = this[i];

                if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                    return attribute.Value;
            }

            return null;
        }

        public IEnumerable<ISdpAttribute> GetAttributes(string name)
        {
            for (var index = 0; index < Count; index++)
            {
                var attribute = this[index];

                if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return attribute;
                }
            }
        }

        public T? GetAttribute<T>(string name) where T : class, ISdpAttribute
        {
            for (var i = 0; i < Count; i++)
            {
                var attribute = this[i];

                if (attribute is T typed && string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                    return typed;
            }

            return null;
        }

        public IEnumerable<T> GetAttributes<T>(string name) where T : class, ISdpAttribute
        {
            for (var index = 0; index < Count; index++)
            {
                var attribute = this[index];

                if (attribute is T typed && string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    yield return typed;
                }
            }
        }
    }
}
