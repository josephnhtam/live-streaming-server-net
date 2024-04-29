namespace LiveStreamingServerNet.Flv.Test.Utilities
{
    public static class Helpers
    {
        public static bool Match<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>>? map1, IEnumerable<KeyValuePair<TKey, TValue>>? map2)
        {
            if (map1 == null && map2 == null)
                return true;

            if (map1 == null || map2 == null)
                return false;

            if (map1.Count() != map2.Count())
                return false;

            foreach (var (key, value) in map1)
            {
                if (key == null)
                    continue;

                var otherValue = map2.FirstOrDefault(x => key.Equals(x.Key)).Value;

                if (value == null && otherValue == null)
                    continue;

                if (value == null || otherValue == null)
                    return false;

                if (!value.Equals(otherValue))
                    return false;
            }

            return true;
        }
    }
}
