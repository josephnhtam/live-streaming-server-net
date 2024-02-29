namespace LiveStreamingServerNet.Utilities.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
        => ((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary).ToDictionary();

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
        where TKey : notnull
        => ((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary).ToDictionary();

    private static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dictionary)
        where TKey : notnull
    {
        return dictionary.ToDictionary(k => k.Key, v => v.Value);
    }
}