namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class DirectoryExtensions
    {
        public static TValue? GetValueOrDefault<TValue>(this IDictionary<string, object> dict, string key, TValue? defaultValue = default)
        {
            if (dict.TryGetValue(key, out var value) && value is TValue result)
                return result;

            return defaultValue;
        }
    }
}