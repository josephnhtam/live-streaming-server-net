namespace LiveStreamingServerNet.Rtmp.Internal.Utilities
{
    internal class AmfArray
    {
        public Dictionary<string, object> Map { get; }

        public AmfArray(IEnumerable<KeyValuePair<string, object>> map)
        {
            Map = new Dictionary<string, object>(map);
        }
    }

    internal static class AmfArrayUtilities
    {
        public static AmfArray ToAmfArray(this IEnumerable<KeyValuePair<string, object>> map)
        {
            return new AmfArray(map);
        }
    }
}
