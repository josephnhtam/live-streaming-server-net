using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Utilities.Common
{
    public class HashedItemCollection<TItem> : IItemCollection<TItem>
    {
        private readonly TItem[] _items;

        public HashedItemCollection(int itemCount, Func<int, TItem> itemFactory)
        {
            _items = Enumerable
                .Range(0, Math.Max(1, itemCount))
                .Select(itemFactory)
                .ToArray();
        }

        public TItem Get(string key)
        {
            return _items[Math.Abs(key.GetHashCode()) % _items.Length];
        }
    }
}
