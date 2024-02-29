using mtanksl.ActionMessageFormat;
using LiveStreamingServerNet.Utilities.Extensions;

namespace LiveStreamingServerNet.Rtmp.Internal.Extensions
{
    internal static class AmfWriterExtensions
    {
        public static void WriteAmf0ObjectWithType(this AmfWriter amfWriter, IDictionary<string, object?> keyValuePairs)
        {
            var amf0Object = new Amf0Object();
            amf0Object.ClassName = string.Empty;
            amf0Object.DynamicMembersAndValues = keyValuePairs as Dictionary<string, object?> ?? keyValuePairs.ToDictionary();
            amfWriter.WriteAmf0(amf0Object);
        }

        public static void WriteAmf3ObjectWithType(this AmfWriter amfWriter, IDictionary<string, object?> keyValuePairs)
        {
            var amf0Object = new Amf3Object();
            amf0Object.Trait.IsDynamic = true;
            amf0Object.DynamicMembersAndValues = keyValuePairs as Dictionary<string, object?> ?? keyValuePairs.ToDictionary();
            amfWriter.WriteAmf0(amf0Object);
        }
    }
}
