using mtanksl.ActionMessageFormat;

namespace LiveStreamingServerNet.Rtmp.Internal.Extensions
{
    internal static class AmfWriterExtensions
    {
        public static void WriteAmf0ObjectWithType(this AmfWriter amfWriter, IEnumerable<KeyValuePair<string, object?>> keyValuePairs)
        {
            var amf0Object = new Amf0Object()
            {
                ClassName = string.Empty,
                DynamicMembersAndValues = keyValuePairs as Dictionary<string, object?> ?? new Dictionary<string, object?>(keyValuePairs)
            };
            amfWriter.WriteAmf0(amf0Object);
        }

        public static void WriteAmf3ObjectWithType(this AmfWriter amfWriter, IEnumerable<KeyValuePair<string, object?>> keyValuePairs)
        {
            var amf3Object = new Amf3Object()
            {
                Values = new List<object?>(),
                Trait = new Amf3Trait { ClassName = string.Empty, IsDynamic = true, Members = new List<string>() },
                DynamicMembersAndValues = keyValuePairs as Dictionary<string, object?> ?? new Dictionary<string, object?>(keyValuePairs)
            };
            amfWriter.WriteAmf3(amf3Object);
        }
    }
}
