using mtanksl.ActionMessageFormat;

namespace LiveStreamingServer.Rtmp.Core.Extensions
{
    public static class AmfWriterExtensions
    {
        public static void WriteAmf0ObjectWithType(this AmfWriter amfWriter, IDictionary<string, object?> keyValuePairs)
        {
            var amf0Object = new Amf0Object();
            amf0Object.ClassName = string.Empty;
            amf0Object.DynamicMembersAndValues = keyValuePairs is Dictionary<string, object?> dict ? dict : keyValuePairs.ToDictionary();
            amfWriter.WriteAmf0(amf0Object);
        }

        public static void WriteAmf3ObjectWithType(this AmfWriter amfWriter, IDictionary<string, object?> keyValuePairs)
        {
            var amf0Object = new Amf3Object();
            amf0Object.Trait.IsDynamic = true;
            amf0Object.DynamicMembersAndValues = keyValuePairs is Dictionary<string, object?> dict ? dict : keyValuePairs.ToDictionary();
            amfWriter.WriteAmf0(amf0Object);
        }
    }
}
