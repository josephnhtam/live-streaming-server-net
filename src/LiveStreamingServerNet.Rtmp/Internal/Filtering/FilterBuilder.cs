using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Filtering.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Filtering
{
    internal class FilterBuilder<TItem> : IFilterBuilder<TItem> where TItem : struct
    {
        private readonly HashSet<TItem> _include;
        private readonly HashSet<TItem> _exclude;

        public FilterBuilder()
        {
            _include = new HashSet<TItem>();
            _exclude = new HashSet<TItem>();
        }

        public IFilterBuilder<TItem> Include(TItem codec)
        {
            _exclude.Remove(codec);
            _include.Add(codec);
            return this;
        }

        public IFilterBuilder<TItem> Exclude(TItem codec)
        {
            _include.Remove(codec);
            _exclude.Add(codec);
            return this;
        }

        public IFilter<TItem> Build()
        {
            return new Filter<TItem>(_include, _exclude);
        }
    }
}
