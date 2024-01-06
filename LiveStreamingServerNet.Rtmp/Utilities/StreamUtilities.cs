using System.Web;

namespace LiveStreamingServerNet.Rtmp.Utilities
{
    internal static class StreamUtilities
    {
        public static (string, IDictionary<string, string>) ParseStreamPath(string streamNameWithQueryString)
        {
            var publishingNameSplit = streamNameWithQueryString.Split('?');

            var streamName = publishingNameSplit[0];

            var queryString = publishingNameSplit.Length > 1 ? publishingNameSplit[1] : string.Empty;
            var queryStringCollection = HttpUtility.ParseQueryString(queryString);
            var queryStringMap = queryStringCollection
                .AllKeys
                .Where(key => !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(queryStringCollection[key]))
                .ToDictionary(key => key!, key => queryStringCollection[key]!);

            return (streamName, queryStringMap);
        }
    }
}
